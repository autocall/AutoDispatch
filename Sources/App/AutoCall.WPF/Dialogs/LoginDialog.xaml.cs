using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using AutoCall.Common;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Settings;
using AutoCall.Logic.ViewModels;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Logic.Services;

namespace AutoCall.WPF.Dialogs {
    /// <summary>
    ///     Interaction logic for LoginDialog.xaml.
    ///     V1 входил дважды: в AutoCall (IApiManager.LoginAsync — сессия, модули, SIP-учётка, DefaultPassword) и, если есть модуль
    ///     AutoDispatch, в dispatch (Bootstrapper Unity + ILoginManager.Login, OAuth Api/Token). Теперь один вход Api/Account/SignIn
    ///     (LoginService), после него — настройки пользователя (Api/Settings) </summary>
    public partial class LoginDialog : SingleInstanceWindow {
        #region [ Private Fields ]

        /// <summary>
        ///     Вход (V1 — IApiManager AutoCall и ILoginManager dispatch) </summary>
        private LoginService _loginService;

        /// <summary>
        ///     Settings service </summary>
        private SettingsService _settings;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets view model </summary>
        public LoginViewModel Model {
            get { return DataContext as LoginViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public LoginDialog(LoginService loginService, SettingsService settings) {
            InitializeComponent();

            string beta = VersionHelper.IsBeta() ? " Beta" : String.Empty;
            Title = $"AutoDispatch v.{VersionHelper.GetDisplayVersion()}{beta}";

            _settings = settings;
            _loginService = loginService;

            Initialize();

#if DEBUG
            // V1: учётка разработчика прямо в коде; теперь — Settings.cs (в .gitignore), как AutoCall V3
            if (String.IsNullOrEmpty(AutoCall.Common.Settings.DebugUserName) == false) {
                Loaded += (sender, e) => {
                    Model.UserName = AutoCall.Common.Settings.DebugUserName;
                    Model.Password = AutoCall.Common.Settings.DebugPassword;
                    btnOK.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                };
            }
#endif
        }

        #endregion [ .ctor ]

        #region [ Private Methods ]

        /// <summary>
        ///     Initializes dialog</summary>
        private void Initialize() {
            // Get last entered user name from settings file
            string userName = _settings.GetUserName();

            // Create view model
            var viewModel = new LoginViewModel() {
                UserName = userName,
                Password = String.Empty
            };

            // Set focus no necessary element
            if (String.IsNullOrEmpty(viewModel.UserName)) {
                tbUserName.Focus();
            } else {
                tbPassword.Focus();
            }

            // Set view model to data context
            Model = viewModel;
        }

        /// <summary>
        ///     On OK button clicked </summary>
        private async void OnOKButtonClicked(object sender, RoutedEventArgs e) {
            // Check is model valid
            if (!Model.IsValid)
                return;
            Model.State = LoginState.LoginStarted;

            var userName = Model.UserName.ToLower().Trim();
            var version = VersionHelper.GetDisplayVersion();

            var result = await _loginService.LoginAsync(userName, Model.Password, version);
            if (result.Successed) {
                try {
                    // настройки пользователя (Api/Settings) — до MainWindow: борды читают их синхронно
                    await _settings.LoadAsync();
                } catch (Exception exception) {
                    _webl.e("Login: settings", exception);
                    result.ErrorResult(exception);
                }
            }

            // Set login state
            Model.State = result.Successed ? (LoginState)(int)result.State : LoginState.LoginFailed;
            // Set error message if failed
            if (result.Successed == false) {
                Model.ErrorMessage = result.Exception?.Message;
            }
            // V1: если пароль — пароль по умолчанию (флаг DefaultPassword входа AutoCall), открывался ChangePasswordDialog.
            // Сервер AutoDispatch, как и AutoCall V3, такого флага не отдаёт — мёртвый код V1:
            //if (result.DefaultPassword) {
            //    var dialog = new ChangePasswordDialog(_mainManager, _manager, _settings);
            //    if (dialog.ShowDialog() == false) {
            //        DialogResult = false;
            //        return;
            //    }
            //}
            if (Model.LoginSuccessed) {
                // Save user name into settings file
                _settings.SetUserName(userName);

                // Go to main window
                DialogResult = true;
            } else if (Model.LoginFailed) {
                tbPassword.Focus();
                tbPassword.SelectAll();
            }
        }

        /// <summary>
        ///     On Cancel button clicked </summary>
        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            // Close application
            Close();
        }

        private void ForgotPasswordOnClick(object sender, RoutedEventArgs e) {
            var dialog = new ForgotPasswordDialog(_loginService);
            dialog.Model = new ForgotPasswordViewModel() { Email = Model.UserName };
            dialog.ShowDialog();
        }

        #endregion [ Private Methods ]
    }
}
