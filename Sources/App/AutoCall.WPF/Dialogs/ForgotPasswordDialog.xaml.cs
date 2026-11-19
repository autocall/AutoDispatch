using System.Windows;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.ViewModels;
using AutoCall.WPF.Extensions;

namespace AutoCall.WPF.Dialogs {
    public partial class ForgotPasswordDialog : Window {
        #region [ Private Fields ]

        /// <summary>
        ///     V1 — MainManager.ForgotPasswordAsync модуля AutoCall; теперь Api/Account/ForgotPassword через LoginService </summary>
        private AutoDispatch.Logic.Services.LoginService _loginService;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets view model </summary>
        public ForgotPasswordViewModel Model {
            get { return DataContext as ForgotPasswordViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public ForgotPasswordDialog(AutoDispatch.Logic.Services.LoginService loginService) {
            InitializeComponent();

            _loginService = loginService;

            Initialize();
        }

        #endregion [ .ctor ]

        #region [ Private Methods ]

        /// <summary>
        ///     Initializes dialog </summary>
        private void Initialize() {
            tbEmail.Focus();
        }

        private async void OnOKButtonClicked(object sender, RoutedEventArgs e) {
            Model.ErrorMessage = null;
            if (!Model.Validate()) {
                return;
            }

            Model.State = LoginState.LoginStarted;

            var result = await _loginService.ForgotPasswordAsync(Model.Email);
            if (result.Successed) {
                Model.State = LoginState.LoggedIn;
                MessageBoxEx.Info(result.Message);
                Close();
            } else {
                Model.State = LoginState.LoginFailed;
                Model.ErrorMessage = result.Exception.Message;
            }
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        #endregion [ Private Methods ]
    }
}