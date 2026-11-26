using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Dto;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoCall.Logic.Services {

    // ref \MicroSIP\pjproject-2.8\pjsip\include\pjsip-ua\sip_inv.h
    internal enum pjsip_inv_state {
        PJSIP_INV_STATE_NULL,           /**< Before INVITE is sent or received   */
        PJSIP_INV_STATE_CALLING,        /**< After INVITE is sent		         */
        PJSIP_INV_STATE_INCOMING,       /**< After INVITE is received.	         */
        PJSIP_INV_STATE_EARLY,          /**< After response with To tag.	     */
        PJSIP_INV_STATE_CONNECTING,     /**< After 2xx is sent/received.	     */
        PJSIP_INV_STATE_CONFIRMED,      /**< After ACK is sent/received.	     */
        PJSIP_INV_STATE_DISCONNECTED,   /**< Session is terminated.		         */
        // added
        PJSUA_CALL_MEDIA_LOCAL_HOLD,
        // added
        REQUEST_ACCOUNT
    }

    internal enum microsip_action {
        MAKE_CALL,
        HANGUP,
        HANGUP_ALL,
        HOLD,
        RESUME,
        TRANSFER,
        ACTION_DTMF,
        SET_ACCOUNT
    }

    internal class MicroSIPAction {
        public int action { get; set; }
        public string phone { get; set; }
    }

    internal class MicroSIPState {
        public int state { get; set; }
        // example: 8001231234
        public string phone { get; set; }
    }

    public class MicroSIPCallService : CallService, ICallService, IDisposable {

        private object _lock = new object();

        /// <summary>
        ///     Мс ожидания канала MicroSIP_Action: autophone.exe запущен — подключение мгновенное (аудит D15) </summary>
        private const int ActionConnectTimeout = 3000;

        private readonly SettingsService _settings;

        public MicroSIPCallService(SettingsService settings) {
            _settings = settings;
        }

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

        private List<PhoneRecordId> _currentCalls = new List<PhoneRecordId>();

        private Dictionary<string, string> _tmpRecordIds = new Dictionary<string, string>();
        private Dictionary<string, string> _tmpDtmf = new Dictionary<string, string>();

        #region [ Properties ]

        public override CallServiceType CallServiceType => CallServiceType.MicroSIP;

        private string MicroSIPLogin { get; set; }
        private string MicroSIPPassword { get; set; }
        private string MicroSIPSettings { get; set; }

        private string MicroSIPAccountJson => JsonExtension.SerializeObject(new {
            login = MicroSIPLogin,
            password = MicroSIPPassword,
            callType = (int)base.CallType
        }, TypeNameHandling.None);

        #endregion [ Properties ]

        /// <summary>
        ///     Takes Login/Password from session of settings </summary>
        public override void SetCallSettings(CallServiceSettings settings) {
            base.SetCallSettings(settings);

            var session = UserSession.Current;

            this.MicroSIPLogin = session.MicroSIPLogin;
            this.MicroSIPPassword = session.MicroSIPPassword;
            this.MicroSIPSettings = settings.MicroSIPSettings;

            StartNotification();
            StartAction(microsip_action.SET_ACCOUNT, MicroSIPAccountJson);
        }

        private bool StartProcess(string phone) {
            return MicroSIPHelper.StartProcess(_settings, phone);
        }

        private string _urlToNumber(string url) {
            if (url == null) return null;
            // NOTES: examples
            // "<sip:8008973343@phone.myautocall.net>"
            // "<sip:8008973343@phone.myautocall.net;transport=tcp>"
            var match = Regex.Match(url, @"^<sip:(?<number>\d{10,11})\@(\w|\.)+(;transport=[\w]+)?>$");
            if (match.Success) {
                return match.Groups["number"].Value;
            }
            return url;
        }

        private void StartAction(microsip_action action, string phone) {
            Task.Factory.StartNew(() => {
                using (NamedPipeClientStream pipeServer = new NamedPipeClientStream(".", "MicroSIP_Action", PipeDirection.Out)) {
                    phone = _urlToNumber(phone);

                    try {
                        // аудит D15: без таймаута Connect ждал вечно, если autophone.exe не запущен, — каждое действие занимало поток пула
                        pipeServer.Connect(ActionConnectTimeout);
                        using (StreamWriter sw = new StreamWriter(pipeServer)) {
                            sw.AutoFlush = true;

                            var obj = new MicroSIPAction() {
                                action = (int)action,
                                phone = phone
                            };
                            sw.WriteLine(JsonExtension.SerializeObject(obj));
                        }
                    } catch (TimeoutException) {
                        Logger.Warning("MicroSIP StartAction {Action}: MicroSIP is not running (no pipe in {Timeout} ms)", action, ActionConnectTimeout);
                    } catch (IOException e) {
                        Logger.Error(e, "MicroSIP StartAction {Action}", action);
                    }
                }
            });
        }

        private bool NotificationStarted = false;

        private void StartNotification() {
            if (NotificationStarted == true) {
                return;
            }
            Task.Factory.StartNew(async () => {
                try {
                    NotificationStarted = true;
                    while (true) {
                        using (NamedPipeClientStream pipeServer = new NamedPipeClientStream(".", "MicroSIP_Nofification", PipeDirection.In)) {
                            pipeServer.Connect();
                            using (StreamReader sr = new StreamReader(pipeServer)) {
                                lock (_lock) {
                                    var json = sr.ReadLine();
                                    if (String.IsNullOrEmpty(json)) continue;
                                    var action = JsonExtension.DeserializeObject<MicroSIPState>(json);
                                    var number = _urlToNumber(action.phone);
                                    Debug.WriteLine($"{action.phone}: {(pjsip_inv_state)action.state}");
                                    //_webl.i($"{action.phone}: {(pjsip_inv_state)action.state}");
                                    var call = _currentCalls.FirstOrDefault(e => e.Phone == action.phone);
                                    switch ((pjsip_inv_state)action.state) {
                                        case pjsip_inv_state.PJSIP_INV_STATE_NULL:
                                            break;
                                        case pjsip_inv_state.PJSIP_INV_STATE_INCOMING:
                                            break;
                                        case pjsip_inv_state.PJSIP_INV_STATE_CALLING:
                                        case pjsip_inv_state.PJSIP_INV_STATE_EARLY: {
                                                if (_tmpRecordIds.ContainsKey(number) == true) {
                                                    var recordId = _tmpRecordIds[number];
                                                    _currentCalls.Add(new PhoneRecordId(action.phone, recordId));
                                                    OnCallChanged(recordId, CallStatus.Calling);
                                                    _tmpRecordIds.Remove(number);
                                                }
                                            }
                                            break;
                                        case pjsip_inv_state.PJSIP_INV_STATE_CONNECTING:
                                            break;
                                        case pjsip_inv_state.PJSIP_INV_STATE_CONFIRMED: {
                                                if (call != null) {
                                                    OnCallChanged(call.RecordId, CallStatus.Established);
                                                }
                                                if (_tmpDtmf.ContainsKey(number) == true) {
                                                    var ts = TaskScheduler.Current;
                                                    // NOTES: without 'await'
                                                    Task.Factory.StartNew(async () => {
                                                        await Task.Delay(TimeSpan.FromSeconds(2));
                                                        Task.Factory.StartNew(async () => {
                                                            if (_currentCalls.Contains(call)) {
                                                                StartAction(microsip_action.ACTION_DTMF, _tmpDtmf[number]);
                                                            }
                                                            _tmpDtmf.Remove(number);
                                                        }, new CancellationToken(), TaskCreationOptions.None, ts);
                                                    });
                                                }
                                            }
                                            break;
                                        case pjsip_inv_state.PJSIP_INV_STATE_DISCONNECTED:
                                            if (call != null) {
                                                OnCallChanged(call.RecordId, CallStatus.CallEnded);
                                                _currentCalls.Remove(call);
                                            }
                                            if (_currentCalls.Any()) {
                                                StartAction(microsip_action.RESUME, _currentCalls.First().Phone);
                                            }
                                            break;
                                        case pjsip_inv_state.PJSUA_CALL_MEDIA_LOCAL_HOLD:
                                            if (call != null) {
                                                OnCallChanged(call.RecordId, CallStatus.EstablishedHold);
                                            }
                                            break;
                                        case pjsip_inv_state.REQUEST_ACCOUNT:
                                            StartAction(microsip_action.SET_ACCOUNT, MicroSIPAccountJson);
                                            break;
                                    }
                                    OnPhoneStatusChanged(_currentCalls.Count > 0 || _tmpRecordIds.Count > 0);
                                }
                                await Task.Delay(TimeSpan.FromSeconds(0.1));
                            }
                        }
                    }
                } catch (Exception e) {
                    Logger.Error(e, "MicroSIP StartNotification");
                } finally {
                    NotificationStarted = false;
                }
            });
        }

        public override async Task<bool> StartCallAsync(string phoneNumber, string recordId = null, string company = null, string origin = null, string destination = null, object model = null) {
            await base.StartCallAsync(phoneNumber, recordId, company);
            lock (_lock) {
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

                if (StartProcess(phone) == false) {
                    StartAction(microsip_action.MAKE_CALL, phone);
                }

                return true;
            }
        }

        public override async Task HangCallAsync() {
            await base.HangCallAsync();
            lock (_lock) {

                StartAction(microsip_action.HANGUP_ALL, null);

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
