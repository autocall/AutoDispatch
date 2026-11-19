using AutoCall.Common.Extensions;
using AutoCall.Logic.Enums;
using System;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents login view model </summary>
    public class ForgotPasswordViewModel : BaseViewModel {

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

        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets error message </summary>|
        public string ErrorMessage {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool Validate() {

            if (String.IsNullOrEmpty(Email)) {
                ErrorMessage = "Enter a Email";
                return false;
            }

            if (StringExtension.IsValidEmail(Email) == false) {
                ErrorMessage = "Enter a valid Email";
                return false;
            }

            return true;
        }

        #endregion [ Properties ]
    }
}