using System;
using System.Threading.Tasks;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Dto;
using AutoCall.Logic.Enums;

namespace AutoCall.Logic.Services {

    public delegate void AbtoPhoneCallServiceOpenDetails2EventHandler(string loadId);
    /// <summary>
    ///     Occurs when phone status changed </summary>
    public delegate void CallServicePhoneStatusChangedEventHandler(bool calling);
    /// <summary>
    ///     Occurs when call changed </summary>
    public delegate void CallServiceCallChangedEventHandler(string recordId, CallStatus status, int? line);
    /// <summary>
    ///     Occurs when phone ringing </summary>
    public delegate void CallServiceCallRingingEventHandler(string phone, string displayName);

    /// <summary>
    ///     Represents call service interface </summary>
    public interface ICallService : IDisposable {

        #region [ Properties ]

        /// <summary>
        ///     Gets call service type </summary>
        CallServiceType CallServiceType { get; }

        /// <summary>
        ///     AutoCall Enable </summary>
        bool AutoCallEnable { get; set; }

        #endregion [ Properties ]

        #region [ Public ]

        /// <summary>
        ///     Sets call service  settings </summary>
        void SetCallSettings(CallServiceSettings settings);

        /// <summary>
        ///     Call number </summary>
        /// <param name="record">Dto</param>
        /// <param name="useComment">Use phone and extension from Comment</param>
        /// <param name="cancelable">Allow cancel call</param>
        // Record-перегрузки V1 (StartCallAsync, HangCallAsync, Hold/Resume/TransferCall по записи DAT) — модуль AutoCall, не перенесены

        /// <summary>
        ///     Starts call </summary>
        Task<bool> StartCallAsync(
            string phoneNumber, 
            string recordId = null, 
            string company = null, 
            string origin = null, 
            string destination = null,
            object model = null);
        /// <summary>
        ///     Hangs current call </summary>
        Task HangCallAsync();

        /// <summary>
        ///     Hangs call </summary>
        /// <summary>
        ///     Holds call </summary>
        /// <summary>
        ///     Resumes call </summary>
        /// <summary>
        ///     Transfers call </summary>
        /// <summary>
        ///     Normalize phone number </summary>
        string ToPhoneNumber(string phone);

        #endregion [ Public ]

        #region [ Events ]

        /// <summary>
        ///     Phone Status changed </summary>
        event CallServicePhoneStatusChangedEventHandler PhoneStatusChanged;
        /// <summary>
        ///     Call changed </summary>
        event CallServiceCallChangedEventHandler CallChanged;
        /// <summary>
        ///     Call changed </summary>
        event CallServiceCallRingingEventHandler CallRinging;

        #endregion [ Events ]
    }
}