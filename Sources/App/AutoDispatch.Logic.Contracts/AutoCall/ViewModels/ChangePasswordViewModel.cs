using AutoCall.Logic.Enums;
using System;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents login view model </summary>
    public class ChangePasswordViewModel : BaseViewModel {
        #region [ Private Fields ]

        /// <summary>
        ///     Login state </summary>
        private LoginState _state = LoginState.LoggedOut;
        /// <summary>
        ///     Error message </summary>
        private string _errorMessage;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets login state </summary>
        public LoginState State {
            get { return _state; }
            set {
                _state = value;
                OnPropertyChanged();
            }
        }

        public string Password { get; set; }
        public string VerifyPassword { get; set; }

        /// <summary>
        ///     Gets or sets error message </summary>
        public string ErrorMessage {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool Validate() {
            if (String.IsNullOrEmpty(Password)) {
                ErrorMessage = "Enter a passwerd";
                return false;
            }
            if (Password.Length < 6) {
                ErrorMessage = "Passwords must be at least 6 characters long";
                return false;
            }
            if (String.IsNullOrEmpty(VerifyPassword)) {
                ErrorMessage = "Repeat password";
                return false;
            }
            if (Password != VerifyPassword) {
                ErrorMessage = "The password must match";
                return false;
            }
            return true;
        }

        #endregion [ Properties ]
    }
}