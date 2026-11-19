using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Dto;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Helpers;
using AutoCall.Logic.Managers;
using AutoCall.Logic.ViewModels;
using MlSampleWindowCS.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoCall.Logic.Services {

    public class AbtoPhoneCallService : CallService, ICallService, IDisposable {

        private MainManager _mainManager;
        private AutoPhoneV2CallService _callService;

        private object _lock = new object();

        [DllImport("User32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        private const uint SW_RESTORE = 0x09;
        [DllImport("User32.dll", SetLastError = true)]
        static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        private class PhoneRecordId {
            public string Phone { get; }
            public string RecordId { get; }
            public PhoneRecordId(string phone, string recordId) {
                this.Phone = phone;
                this.RecordId = recordId;
            }
        }

        private static List<PhoneRecordId> _currentCalls = new List<PhoneRecordId>();

        private static Dictionary<string, string> _tmpRecordIds = new Dictionary<string, string>();
        private static Dictionary<string, string> _tmpDtmf = new Dictionary<string, string>();

        #region [ Properties ]

        public override CallServiceType CallServiceType => CallServiceType.AbtoPhone;

        private string AbtoPhoneLogin { get; set; }
        private string AbtoPhonePassword { get; set; }
        private string AbtoPhoneSettings { get; set; }
        private string Shortcuts { get; set; }
        private string Contacts { get; set; }

        private string AbtoPhoneAccountJson => JsonExtension.SerializeObject(new {
            login = AbtoPhoneLogin,
            password = AbtoPhonePassword,
            callType = (int)base.CallType,
            maxPhoneLines = (int)base.MaxPhoneLines,
            shortcuts = Shortcuts,
            contacts = Contacts,
        }, TypeNameHandling.None);

        #endregion [ Properties ]

        public AbtoPhoneCallService(MainManager mainManager, AutoPhoneV2CallService callService) {
            _mainManager = mainManager;
            _callService = callService;
        }

        /// <summary>
        ///     Takes Login/Password from session of settings </summary>
        public override void SetCallSettings(CallServiceSettings settings) {
            base.SetCallSettings(settings);

            var session = UserSession.Current;
            this.AbtoPhoneLogin = session.MicroSIPLogin;
            this.AbtoPhonePassword = session.MicroSIPPassword;
            this.AbtoPhoneSettings = settings.MicroSIPSettings;

            SettingsCallServiceViewModel callSettings = _mainManager.GetCallServiceSettings();
            this.Shortcuts = JsonExtension.SerializeObject(callSettings.MicroSIPSettings?.Shortcuts, TypeNameHandling.None);
            this.Contacts = JsonExtension.SerializeObject(callSettings.MicroSIPSettings?.Contacts, TypeNameHandling.None);

            StartNotification();
        }

        private bool StartProcess() {
            return AbtoPhoneHelper.StartProcess(_mainManager.Settings, _mainManager.AutoPhoneV2CallService);
        }

        private string _urlToNumber(string url) {
            if (url == null) return null;
            // NOTES: examples
            // "sip:8008973343@phone.myautocall.net"
            // "sip:8008973343@phone.myautocall.net;transport=tcp"
            var match = Regex.Match(url, @"^sip:(?<number>\d{10,11})\@(\w|\.)+(;transport=[\w]+)?$");
            if (match.Success) {
                return match.Groups["number"].Value;
            }
            return url;
        }

        private void StartAction(abtophone_action action, string phone, string header = null, string subHeader = null, string loadId = null, object model = null) {
            _callService.StartAction(action, phone, header, subHeader, loadId, model);
        }

        public void StartNotification() {
            _callService.Notification =
                new Action<abtophone_state /*state*/, string /*phone*/, int? /*line*/, object>(async (state, phone, line, model) => {
                    line = line + 1;
                    var call = _currentCalls.FirstOrDefault(e => e.Phone == /*action.phone*/phone);//NOTES: немогу вытянуть address 
                    switch (state) {
                        case abtophone_state.Ringing:
                        case abtophone_state.Connecting: {
                                if (phone != null && _tmpRecordIds.ContainsKey(phone) == true) {
                                    var recordId = _tmpRecordIds[phone];
                                    _currentCalls.Add(new PhoneRecordId(/*action.phone*/phone, recordId));
                                    OnCallChanged(recordId, CallStatus.Calling, line);
                                    _tmpRecordIds.Remove(phone);
                                }
                            }
                            break;
                        case abtophone_state.Connected: {
                                if (call != null) {
                                    OnCallChanged(call.RecordId, CallStatus.Established, line);
                                }
                                if (phone != null && _tmpDtmf.ContainsKey(phone) == true) {
                                    var ts = TaskScheduler.Current;
                                    // NOTES: without 'await'
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    Task.Factory.StartNew(async () => {
                                        await Task.Delay(TimeSpan.FromSeconds(2));
                                        Task.Factory.StartNew(async () => {
                                            if (_currentCalls.Contains(call)) {
                                                StartAction(abtophone_action.ACTION_DTMF, phone, _tmpDtmf[phone]);
                                            }
                                            _tmpDtmf.Remove(phone);
                                        }, new CancellationToken(), TaskCreationOptions.None, ts);
                                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                }
                            }
                            break;
                        case abtophone_state.Transferred:
                        case abtophone_state.Ended:
                            if (call != null) {
                                if (state == abtophone_state.Ended) {
                                    OnCallChanged(call.RecordId, CallStatus.CallEnded, line);
                                }
                                if (state == abtophone_state.Transferred) {
                                    OnCallChanged(call.RecordId, CallStatus.Transferred, line);
                                }
                                _currentCalls.Remove(call);
                            }
                            if (_currentCalls.Any()) {
                                StartAction(abtophone_action.RESUME, _currentCalls.First().Phone);
                            }
                            break;
                        case abtophone_state.Hold:
                            if (call != null) {
                                OnCallChanged(call.RecordId, CallStatus.EstablishedHold, line);
                            }
                            break;
                        case abtophone_state.SET_ACCOUNT:
                            StartAction(abtophone_action.SET_ACCOUNT, AbtoPhoneAccountJson);
                            break;
                        case abtophone_state.DETAILS:
                            // мёртвый код в App: model — DatPowerRecordViewModel записи DAT (модуль AutoCall), из dispatch приходит только loadId
                            //if (model != null) {
                            //    _mainManager.OpenDetails((DatPowerRecordViewModel)model); // model ViewModel
                            //} else
                            if (phone != null) {
                                _mainManager.OpenDetails2(phone); // phone = loadId
                            }
                            break;
                    }
                    OnPhoneStatusChanged(_currentCalls.Count > 0 || _tmpRecordIds.Count > 0);
                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                });
        }

        public override async Task<bool> StartCallAsync(
                string phoneNumber,
                string recordId = null,
                string company = null,
                string origin = null,
                string destination = null,
                object model = null) {
            await base.StartCallAsync(phoneNumber);
            if (base.CallType == CallType.SingleCall) {
                if (_currentCalls.Any()) {
                    return false;
                }
            } else
            if (base.CallType == CallType.MultiCall) {
                if (_currentCalls.Count >= MaxPhoneLines) {
                    return false;
                }
            } else {
                return false;
            }

            // Get valid phone number ( parse to country code, region, local and extension numbers)
            PhoneNumberDto phoneDto = GetValidPhoneNumber(phoneNumber);
            if (phoneDto == null)
                return false;
            // Dial country code +1
            string phone = DialCountryCode ? phoneDto.InternationalPhoneNumber : phoneDto.PhoneNumber;

            if (_currentCalls.Any(e => e.Phone.Contains(phone)) || _tmpRecordIds.ContainsKey(phone)) {
                return false;
            }

            _tmpRecordIds[phone] = recordId;

            if (string.IsNullOrEmpty(phoneDto.Extension) == false)
                _tmpDtmf[phone] = phoneDto.Extension;

            if (StartProcess() == true) {
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                StartAction(abtophone_action.SET_ACCOUNT, AbtoPhoneAccountJson);
            }
            StartAction(abtophone_action.MAKE_CALL, phone, company, $"{origin}→{destination}", recordId, model);

            return true;
        }

        public override async Task HangCallAsync() {
            await base.HangCallAsync();
            lock (_lock) {

                StartAction(abtophone_action.HANGUP_ALL, null);

                if (_currentCalls.Any() || _tmpRecordIds.Any()) {
                    OnCallChanged(null, CallStatus.CallEnded);
                    foreach (var call in _currentCalls) {
                        OnCallChanged(call.RecordId, CallStatus.CallEnded);
                    }
                    foreach (var call in _tmpRecordIds) {
                        OnCallChanged(call.Value, CallStatus.CallEnded);
                    }
                    _currentCalls.Clear();
                    // возможно надо перенести за if
                    OnPhoneStatusChanged(_currentCalls.Count > 0 || _tmpRecordIds.Count > 0);
                    _tmpRecordIds.Clear();
                }
            }
        }




    }
}
