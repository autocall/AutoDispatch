using System;
using System.Collections.Generic;
using System.Windows;
using AutoCall.Logic.Contracts.Settings;
using AutoCall.Logic.Converters;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Services;
using AutoCall.Logic.Settings;
using AutoCall.Logic.ViewModels;
using MlSampleWindowCS.Services;

namespace AutoCall.Logic.Managers {
    /// <summary>
    ///     MainManager V1 модуля AutoCall, только софтфоны (AI/wiki/plan.md «Софтфоны в App»): набор сервисов звонка, текущий,
    ///     настройки Call Services и события звонка для MainWindow. DAT, источники данных, звук, хоткеи, вход AutoCall — не перенесены.
    ///     IMainManager убран — конкретный класс (DI без интерфейсов) </summary>
    public class MainManager {

        #region [ Private Fields ]

        private Dictionary<CallServiceType, ICallService> _callServices;

        private readonly SettingsService settingsService;
        private readonly AutoPhoneV2CallService _autoPhoneV2CallService;

        #endregion [ Private Fields ]

        #region [ Properties ]

        public Dictionary<CallServiceType, ICallService> CallServices => _callServices;

        public ICallService GetCallService => _callServices != null && _callServices.ContainsKey(CurrentCallService) ?
                                              _callServices[CurrentCallService] : null;

        public CallServiceType CurrentCallService { get; set; }

        /// <summary>
        ///     Настройки для AbtoPhoneHelper (V1 брал их через ServiceLocator) </summary>
        public SettingsService Settings => settingsService;

        /// <summary>
        ///     Мост к форме AutoPhone v.2 для AbtoPhoneHelper </summary>
        public AutoPhoneV2CallService AutoPhoneV2CallService => _autoPhoneV2CallService;

        #endregion [ Properties ]

        #region [ .ctor ]

        public MainManager(SettingsService settingsService, AutoPhoneV2CallService autoPhoneV2CallService) {
            this.settingsService = settingsService;
            _autoPhoneV2CallService = autoPhoneV2CallService;
            // ContactsForm AutoPhone v.2 читает и пишет контакты через MainManager (V1 — ServiceLocator<IMainManager>)
            _autoPhoneV2CallService.GetCallServiceSettings = GetCallServiceSettings;
            _autoPhoneV2CallService.SetCallServiceSettings = SetCallServiceSettings;
            CurrentCallService = CallServiceType.Unknown;
        }

        #endregion [ .ctor ]

        #region [ Public Methods ]

        public void Initialize() {
            _callServices = new Dictionary<CallServiceType, ICallService>();
        }

        public void InitializeCallServices() {
            _callServices[CallServiceType.Unknown] = new NoneCallService();
            _callServices[CallServiceType.MicroSIP] = new MicroSIPCallService(settingsService);
            _callServices[CallServiceType.AbtoPhone] = new AbtoPhoneCallService(this, _autoPhoneV2CallService);
        }

        /// <summary>
        ///     Init call service </summary>
        public void InitializeCallService(CallServiceSettings settings) {
            if (_callServices.ContainsKey(CurrentCallService) && _callServices[CurrentCallService] != null) {
                _callServices[CurrentCallService].CallChanged -= OnCallChanged;
                _callServices[CurrentCallService].PhoneStatusChanged -= OnPhoneStatusChanged;
            }
            CurrentCallService = settings.CallService;
            if (_callServices[CurrentCallService] != null) {
                _callServices[CurrentCallService].SetCallSettings(settings);
                _callServices[CurrentCallService].CallChanged += OnCallChanged;
                _callServices[CurrentCallService].PhoneStatusChanged += OnPhoneStatusChanged;
            }
        }

        public SettingsCallServiceViewModel GetCallServiceSettings() {
            CallServiceSettings settings = settingsService.GetCallServiceSettings();
            return CallServiceSettingsConverter.ToVm(settings);
        }

        public void SetCallServiceSettings(SettingsCallServiceViewModel settingsViewModel) {
            var settings = CallServiceSettingsConverter.ToSettings(settingsViewModel);
            settingsService.SetCallServiceSettings(settings);
            settingsService.SaveSettings();
        }

        #endregion [ Public Methods ]

        #region [ Events ]

        // аудит D16: при закрытии App Application.Current — null, `await null` давал NullReferenceException в async void
        private async void OnCallChanged(string recordId, CallStatus status, int? line) {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted) return;
            await dispatcher.InvokeAsync(new Action(() => CallChanged?.Invoke(recordId, status, line)));
        }
        private async void OnPhoneStatusChanged(bool calling) {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted) return;
            await dispatcher.InvokeAsync(new Action(() => PhoneStatusChanged?.Invoke(calling)));
        }

        public event CallServicePhoneStatusChangedEventHandler PhoneStatusChanged;

        public event CallServiceCallChangedEventHandler CallChanged;

        /// <summary>
        ///     Кнопка Details в AutoPhone v.2 по loadId. Карточка записи DAT (V1 OpenDetails(DatPowerRecordViewModel)) — модуль AutoCall, не перенесена </summary>
        public AbtoPhoneCallServiceOpenDetails2EventHandler OpenDetails2 { get; set; }

        #endregion [ Events ]
    }
}
