using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Converters;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Helpers;
using AutoCall.Logic.Managers;
using AutoCall.Logic.ViewModels;
using AutoDispatch.Data;

namespace AutoCall.WPF.Dialogs {
    /// <summary>
    ///     Настройки софтфона — только вкладка Call Services (AI/wiki/plan.md «Софтфоны в App»); вкладки Sound и Hotkeys V1 — модуль
    ///     AutoCall (звук уведомлений DAT, хоткеи звонков), не перенесены. Код вкладки Call Services — V1 </summary>
    public partial class SettingsDialog : Window {

        #region [ Private Fields ]

        /// <summary>
        ///     Main manager instance </summary>
        private MainManager _mainManager;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets view model </summary>
        public SettingsViewModel Model {
            get { return DataContext as SettingsViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Gets CallServiceViewModel from View Model </summary>
        private SettingsCallServiceViewModel CallServiceViewModel => Model.SettingsCallServiceViewModel;

        #endregion [ Properties ]

        #region [ Events ]

        /// <summary>
        ///     Occur when settings was be saved </summary>
        public event EventHandler OnSaveSettings;

        #endregion [ Events ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public SettingsDialog(MainManager mainManager) {
            _mainManager = mainManager;
            Model = new SettingsViewModel();
            Model.SettingsCallServiceViewModel = mainManager.GetCallServiceSettings();
            // Sets all call services
            Model.SettingsCallServiceViewModel.CallServices = mainManager.CallServices.Keys.ToList();

            InitializeComponent();
        }

        #endregion [ .ctor ]

        /// <summary>
        ///     Loaded handler </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var session = UserSession.Current;
            if (String.IsNullOrEmpty(session.MicroSIPLogin) == false) {
                SipLoginTb.IsEnabled = false;
                Model.SettingsCallServiceViewModel.MicroSIPLogin = session.MicroSIPLogin;
            }
            if (String.IsNullOrEmpty(session.MicroSIPPassword) == false) {
                SipPasswordTb.IsEnabled = false;
                Model.SettingsCallServiceViewModel.MicroSIPPassword = session.MicroSIPPassword;
            }
        }

        private void VodiaLink_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("chrome.exe", "https://phone.myautocall.net/usr_portal.htm") { UseShellExecute = true });
        }

        private void ExtensionLink_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("chrome.exe", "https://chrome.google.com/webstore/detail/lgagpmpgbieimldhcgbjhdebaipmaafk") { UseShellExecute = true });
        }

        private void DeskPinsLink_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://myautocall.net/Download/DeskPins-1.32-setup.exe") { UseShellExecute = true });
        }

        private void ABTO_VoIP_SDKLink_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://myautocall.net/Home/Download") { UseShellExecute = true });
        }

        #region [ Private Methods ]

        /// <summary>
        ///     Ok clicked handler </summary>
        private void BtnOk_OnClick(object sender, RoutedEventArgs e) {
            _mainManager.SetCallServiceSettings(CallServiceViewModel);

            _mainManager.InitializeCallService(CallServiceSettingsConverter.ToSettings(CallServiceViewModel));

            RestartCallService();

            this.IsEnabled = false;
            Close();
            OnSaveSettings?.Invoke(this, e);
        }

        private void RestartCallService() {
            if (CallServiceViewModel.SelectedCallService == CallServiceType.MicroSIP) {
                _ = MicroSIPHelper.TryRestart(_mainManager.Settings);
                _mainManager.InitializeCallServices();
                _mainManager.InitializeCallService(CallServiceSettingsConverter.ToSettings(CallServiceViewModel));
            } else {
                MicroSIPHelper.TryClose();
            }
            if (CallServiceViewModel.SelectedCallService == CallServiceType.AbtoPhone) {
                AbtoPhoneHelper.TryRestart(_mainManager.Settings, _mainManager.AutoPhoneV2CallService);
                _mainManager.InitializeCallServices();
                _mainManager.InitializeCallService(CallServiceSettingsConverter.ToSettings(CallServiceViewModel));
            } else {
                AbtoPhoneHelper.TryClose();
            }
        }

        /// <summary>
        ///     Cancel clicked handler </summary>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        ///     Call test phone nuber </summary>
        private async void Call_Click(object sender, RoutedEventArgs e) {
            await _mainManager.CallServices[CallServiceViewModel.SelectedCallService].StartCallAsync(CallServiceViewModel.TestPhoneNumber);
        }

        private void BtnShortcuts_OnClick(object sender, RoutedEventArgs args) {
            var win = new MicroSIPShortcuts();
            win.Model = (MicroSIPSettingsViewModel)Model.SettingsCallServiceViewModel.MicroSIPSettings?.Clone() ?? new MicroSIPSettingsViewModel();
            if (win.ShowDialog() == true) {
                Model.SettingsCallServiceViewModel.MicroSIPSettings = win.Model;
                _mainManager.SetCallServiceSettings(CallServiceViewModel);
                var settings = CallServiceSettingsConverter.ToSettings(CallServiceViewModel);
                _mainManager.InitializeCallService(settings);
                OnSaveSettings?.Invoke(this, args);

                Close();

                RestartCallService();
            }
        }

        private void BtnContacts_OnClick(object sender, RoutedEventArgs args) {
            var win = new MicroSIPContacts();
            win.Model = (MicroSIPSettingsViewModel)Model.SettingsCallServiceViewModel.MicroSIPSettings?.Clone() ?? new MicroSIPSettingsViewModel();
            if (win.ShowDialog() == true) {
                Model.SettingsCallServiceViewModel.MicroSIPSettings = win.Model;
                _mainManager.SetCallServiceSettings(CallServiceViewModel);
                var settings = CallServiceSettingsConverter.ToSettings(CallServiceViewModel);
                _mainManager.InitializeCallService(settings);
                OnSaveSettings?.Invoke(this, args);

                Close();

                RestartCallService();
            }
        }

        private void CallType_OnClick(object sender, RoutedEventArgs e) {
            switch (Model.SettingsCallServiceViewModel.CallType) {
                case CallType.SingleCall:
                    Model.SettingsCallServiceViewModel.CallType = CallType.MultiCall;
                    break;
                case CallType.MultiCall:
                default:
                    Model.SettingsCallServiceViewModel.CallType = CallType.SingleCall;
                    break;
            }
        }

        #endregion [ Private Methods ]
    }
}
