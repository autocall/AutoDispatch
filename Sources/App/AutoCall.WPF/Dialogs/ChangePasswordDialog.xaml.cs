using System.Windows;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.ViewModels;

namespace AutoCall.WPF.Dialogs {
    /// <summary>
    ///     Мёртвый код в App: V1 открывал диалог только из LoginDialog при пароле по умолчанию (флаг DefaultPassword входа AutoCall);
    ///     сервер AutoDispatch, как AutoCall V3, флага не отдаёт, вызов в LoginDialog закомментирован. Смена пароля V1 шла через
    ///     MainManager.ChangePasswordAsync модуля AutoCall — вызов ниже закомментирован </summary>
    public partial class ChangePasswordDialog : Window {
        #region [ Private Fields ]

        /// <summary>
        ///     Main manager instance </summary>
        private MainManager _mainManager;

        /// <summary>
        ///     Settings service </summary>
        private SettingsService _settings;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets view model </summary>
        public ChangePasswordViewModel Model {
            get { return DataContext as ChangePasswordViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public ChangePasswordDialog(MainManager mainManager, SettingsService settings) {
            InitializeComponent();

            _settings = settings;
            _mainManager = mainManager;

            Initialize();
        }

        #endregion [ .ctor ]

        #region [ Private Methods ]

        /// <summary>
        ///     Initializes dialog </summary>
        private void Initialize() {
            tbPassword.Focus();
            Model = new ChangePasswordViewModel();
        }

        private async void OnOKButtonClicked(object sender, RoutedEventArgs e) {
            Model.ErrorMessage = null;
            if (!Model.Validate()) {
                return;
            }
            Model.State = LoginState.LoginStarted;

            //var result = await _mainManager.ChangePasswordAsync(Model.Password);
            //if (result.Successed) {
            //    Model.State = LoginState.LoggedIn;
            //    DialogResult = true;
            //    Close();
            //} else {
            //    Model.State = LoginState.LoginFailed;
            //    Model.ErrorMessage = result.Exception.Message;
            //}
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e) {
            Close();
        }

        #endregion [ Private Methods ]
    }
}