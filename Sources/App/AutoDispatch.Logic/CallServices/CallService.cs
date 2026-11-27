using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoCall.Common.Loggers;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Dto;
using AutoCall.Logic.Enums;
using AutoMapper;

namespace AutoCall.Logic.Services {
    /// <summary>
    ///     Represents interface for call service </summary>
    // Record-перегрузки V1 (StartCallAsync(Record), HangCallAsync/HoldCall/ResumeCall/TransferCall(Record)) — звонки по записям DAT
    // модуля AutoCall, в AutoDispatch App не перенесены (AI/wiki/plan.md «Софтфоны в App»)
    public abstract class CallService : ICallService {

        #region [ private ]

        /// <summary>
        ///     Scope на сервере: app.sip (как AutoCall V3) </summary>
        protected static Serilog.ILogger Logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Sip);

        /// <summary>
        ///     Regex pattern for phone numbers from DAT Power server </summary>
        private const string PhoneNumberPattern =
            @"^\(?(?<region>\d{3})\D{1,2}(?<localNumberPart1>\d{3})\D?(?<localNumberPart2>\d{4})(?<extension>\D?(\((\d+)\)|x\d+))?$";

        #endregion [ private ]

        #region [ Protected ]

        /// <summary>
        ///      Record ID for saving phone line </summary>
        protected string CallingRecordId;
        /// <summary>
        ///     Gets or setrs Call Type. Multi or Single </summary>
        protected CallType CallType;
        /// <summary>
        ///     Gets or sets maximum phone lines </summary>
        protected byte MaxPhoneLines;
        /// <summary>
        ///     Gets or sets dialing country code </summary>
        protected bool DialCountryCode;

        /// <summary>
        ///     Parse phone number </summary>
        /// <param name="phoneNumber">Input phone number</param>
        /// <returns>Parsed phone number (country code, region, local number, extention)</returns>
        protected PhoneNumberDto GetValidPhoneNumber(string phoneNumber) {
            if (String.IsNullOrEmpty(phoneNumber)) return null;
            Match match = Regex.Match(phoneNumber, PhoneNumberPattern);
            if (!match.Success)
                return null;

            return new PhoneNumberDto() {
                Region = match.Groups["region"].Value,
                LocalNumber = $"{match.Groups["localNumberPart1"].Value}{match.Groups["localNumberPart2"].Value}",
                Extension = StringExtension.OnlyNumbers(match.Groups["extension"].Value),
            };
        }

        /// <summary>
        ///     Convert dto phone number to string </summary>
        protected string ToPhoneNumber(PhoneNumberDto phone) {
            // TODO DEBUG ERROR
            if (phone == null) {
                return "UNDEFINED 1";
            }
            if (phone.InternationalPhoneNumber == null) {
                return "UNDEFINED 2";
            }
            var result = new StringBuilder(19);
            result.Append('(')
                .Append(phone.InternationalPhoneNumber, 1, 3)
                .Append(") ")
                .Append(phone.InternationalPhoneNumber, 4, 3)
                .Append('-')
                .Append(phone.InternationalPhoneNumber, 7, 4);

            if (phone.HasExtension) {
                result.Append($" ({phone.Extension})");
            }

            return result.ToString();
        }

        protected void OnPhoneStatusChanged(bool calling) {
            PhoneStatusChanged?.Invoke(calling);
        }

        /// <summary>
        ///     Raise call changed event </summary>
        protected void OnCallChanged(string recordId, CallStatus status, int? line = null) {
            // метрика (как AutoCall V3): звонок закончен — дозвонился или не ответили (неудачный идёт через OnCallFailed)
            if (status is CallStatus.CallEnded or CallStatus.Transferred) {
                AutoDispatch.Data.Metrics.ClientMetrics.CallEnded(CallServiceType.ToString());
            }
            Logger.Information("Call {Status}. Record {RecordId}, line {Line}, {Service}", status, recordId, line, CallServiceType.GetDescription());
            CallChanged?.Invoke(recordId, status, line);
        }

        /// <summary>
        ///     Звонок оборвался ошибкой (SIP-отказ, сбой софтфона): для борда это тот же конец звонка, в метрике — ошибка (AutoCall V3) </summary>
        protected void OnCallFailed(string recordId, int? line = null) {
            AutoDispatch.Data.Metrics.ClientMetrics.CallFailed(CallServiceType.ToString());
            Logger.Warning("Call failed. Record {RecordId}, line {Line}, {Service}", recordId, line, CallServiceType.GetDescription());
            CallChanged?.Invoke(recordId, CallStatus.CallEnded, line);
        }
        // V1 OnRinging (входящий звонок) поднимал только BriaCallService — не перенесён, событие удалено вместе с панелью входящего звонка

        #endregion [ Protected ]

        #region [ Public ]

        /// <summary>
        ///     AutoCall Enable </summary>
        public bool AutoCallEnable { get; set; } = false;

        /// <summary>
        ///     Sets call service settings </summary>
        public virtual void SetCallSettings(CallServiceSettings settings) {
            this.CallType = settings.CallType;
            this.MaxPhoneLines = settings.MaxPhoneLines;
            this.DialCountryCode = settings.DialCountryCode;
        }

        /// <summary>
        ///     Normalize phone number </summary>
        public string ToPhoneNumber(string phone) {
            if (String.IsNullOrEmpty(phone) || phone == "0") {
                return null;
            }
            // TODO: Check Dial
            if (phone.Length < 10) {
                //_webl.e("Incorrect phone number: " + phone);
                return phone;
            }
            var chars = phone.Where(Char.IsDigit).ToArray();
            return new StringBuilder(14)
                .Append('(')
                .Append(chars, 0, 3)
                .Append(") ")
                .Append(chars, 3, 3)
                .Append('-')
                .Append(chars, 6, 4)
                .ToString();
        }


        public virtual void Dispose() {
        }

        #endregion [ Public ]

        #region [ Abstract ]

        /// <summary>
        ///     Gets call service type </summary>
        public abstract CallServiceType CallServiceType { get; }

        /// <summary>
        ///     Starts call </summary>
        public virtual async Task<bool> StartCallAsync(
                string phoneNumber, string recordId = null,
                string company = null, string origin = null,
                string destination = null, object model = null) {
            CallingRecordId = String.Empty;
            Logger.Information("Call started. Record {RecordId}, {Company}, {Service}", recordId, company, CallServiceType.GetDescription());
            return true;
        }

        /// <summary>
        ///     Hang current call </summary>
        public virtual async Task HangCallAsync() {
            CallingRecordId = String.Empty;
        }





        #endregion [ Abstract ]

        #region [ Events ]

        /// <summary>
        ///     Phone Status changed </summary>
        public event CallServicePhoneStatusChangedEventHandler PhoneStatusChanged;
        /// <summary>
        ///     Call changed </summary>
        public event CallServiceCallChangedEventHandler CallChanged;

        #endregion [ Events ]
    }
}