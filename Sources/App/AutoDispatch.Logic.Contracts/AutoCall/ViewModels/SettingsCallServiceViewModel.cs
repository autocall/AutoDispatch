using System.Collections.Generic;
using AutoCall.Logic.Enums;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents view model for settings window </summary>
    public class SettingsCallServiceViewModel : BaseViewModel {

        #region [ _fields ]

        private List<CallServiceType> _callServices;
        private CallType _callType;
        private bool _dialCountryCode;
        private CallServiceType _selectedCallService;
        private string _microSIPLogin;
        private string _microSIPPassword;

        #endregion [ _fields ]

        #region [ Properties ]

        public List<CallServiceType> CallServices {
            get { return _callServices; }
            set {
                _callServices = value;
                OnPropertyChanged();
            }
        }

        //public string TestPhoneNumber => ConfigurationManager.AppSettings["TestPhoneNumber"];
        public string TestPhoneNumber => "(480) 505-8877";
        /// <summary>
        ///     Number for transfering call </summary>
        public string TransferNumber { get; set; }

        public CallServiceType SelectedCallService {
            get => _selectedCallService;
            set {
                _selectedCallService = value;
                OnPropertyChanged();
            }
        }

        public CallType CallType {
            get { return _callType; }
            set {
                _callType = value;
                OnPropertyChanged();
            }
        }

        public byte MaxPhoneLines { get; set; }

        public bool DialCountryCode {
            get { return _dialCountryCode; }
            set {
                _dialCountryCode = value;
                OnPropertyChanged();
            }
        }

        public string MicroSIPLogin {
            get { return _microSIPLogin; }
            set {
                _microSIPLogin = value;
                OnPropertyChanged();
            }
        }
        public string MicroSIPPassword {
            get { return _microSIPPassword; }
            set {
                _microSIPPassword = value;
                OnPropertyChanged();
            }
        }
        public MicroSIPSettingsViewModel MicroSIPSettings { get; set; }

        #endregion [ Properties ]

        #region [ Methods ]

        /// <summary>
        ///     Override ToString </summary>
        public override string ToString() {
            return $"type: {SelectedCallService}";
        }

        #endregion [ Methods ]
    }
}