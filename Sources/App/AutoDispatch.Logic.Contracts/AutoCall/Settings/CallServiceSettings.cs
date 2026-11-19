using AutoCall.Logic.Enums;

namespace AutoCall.Logic.Contracts.Settings {
    /// <summary>
    ///     Represents DTO for settings window </summary>
    public class CallServiceSettings {
        /// <summary>
        ///     Gets or sets call service type </summary>
        public CallServiceType CallService { get; set; }
        /// <summary>
        ///     Gets or setrs Call Type. Multi or Single </summary>
        public CallType CallType { get; set; }
        /// <summary>
        ///     Gets or sets maximum phone lines </summary>
        public byte MaxPhoneLines { get; set; }
        /// <summary>
        ///     Number for transfering call </summary>
        public string TransferNumber { get; set; }
        /// <summary>
        ///     Always dial county code +1 </summary>
        public bool DialCountryCode { get; set; }

        public string MicroSIPLogin { get; set; }
        public string MicroSIPPassword { get; set; }
        public string MicroSIPSettings { get; set; }
    }
}