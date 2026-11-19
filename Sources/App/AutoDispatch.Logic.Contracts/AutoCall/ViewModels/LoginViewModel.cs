using System;
using System.Configuration;
using AutoCall.Logic.Enums;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents login view model </summary>
    public class LoginViewModel : BaseViewModel {
        #region [ Private Fields ]

        /// <summary>
        ///     Dat Power user name </summary>
        private string _userName;

        /// <summary>
        ///     Dat Power user password </summary>
        private string _password;

        private bool _euProxy;

        /// <summary>
        ///     Login state </summary>
        private LoginState _state;

        /// <summary>
        ///     Error message </summary>
        private string _errorMessage;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets Dat Power user name </summary>
        public string UserName {
            get { return _userName; }
            set {
                _userName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets Dat Power user password </summary>
        public string Password {
            get { return _password; }
            set {
                _password = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State {
            get { return _state; }
            set {
                _state = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     AutoCall API url </summary>
        public Uri NavigateUri { get; set; }

        /// <summary>
        ///     Gets or sets error message </summary>
        public string ErrorMessage {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets is login successed </summary>
        public bool LoginSuccessed {
            get { return _state == LoginState.LoggedIn; }
        }

        /// <summary>
        ///     Gets is login failed </summary>
        public bool LoginFailed {
            get { return _state == LoginState.LoginFailed; }
        }

        /// <summary>
        ///     Is model data valid </summary>
        /// <returns>True - if valid</returns>
        public virtual bool IsValid {
            get {
                if (String.IsNullOrEmpty(UserName)) {
                    ErrorMessage = "User name can not be empty";
                    State = LoginState.LoginFailed;
                    return false;
                }

                if (String.IsNullOrEmpty(Password)) {
                    ErrorMessage = "User password can not be empty";
                    State = LoginState.LoginFailed;
                    return false;
                }
                return true;
            }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public LoginViewModel() {
            UserName = String.Empty;
            Password = String.Empty;
            State = LoginState.LoggedOut;
            ErrorMessage = String.Empty;
            // V1: UrlApi из app.config; ссылка Register в LoginDialog не используется
            NavigateUri = new Uri(AutoCall.Common.Settings.WebUrl + "Account/Register");
        }

        #endregion [ .ctor ]
    }
}