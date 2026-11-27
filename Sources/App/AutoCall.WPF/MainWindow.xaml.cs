using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AutoCall.Common;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Helpers;
using AutoCall.Common.Loggers;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Contracts.Settings.Sections;
using AutoCall.Logic.Converters;
using AutoCall.Logic.Enums;
using AutoCall.Logic.Helpers;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Settings.Sections;
using AutoCall.Logic.ViewModels;
using AutoCall.WPF.Dialogs;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Data;
using AutoDispatch.Data.Api;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Converters;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.Managers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;
using Binding = System.Windows.Data.Binding;
using MessageBox = System.Windows.MessageBox;

namespace AutoCall.WPF {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml.
    ///     V1 — общее окно AutoCall и AutoDispatch; оставлен один модуль: борды Carrier/Broker/Accounting/History по ролям, компании,
    ///     сканер, софтфон (AI/wiki/plan.md этап 7). Не перенесено (модуль AutoCall): борды DatPower, вход в источники DAT/Truckstop,
    ///     NewVersionDialog, тумблер автодозвона AutoCall, QuickRateSearch, Blacklist/Favorite, тип вида, Data Sources, громкость
    ///     уведомлений, хоткеи (вкладка Hotkeys настроек), кнопка админа «Save default columns» (дефолты компании, решено 2026-11-17) </summary>
    public partial class MainWindow : SingleInstanceWindow {
        #region [ Private Fields ]

        /// <summary>
        ///     Main manager instance (софтфоны) </summary>
        private MainManager _manager;

        /// <summary>
        ///     Settings service </summary>
        private SettingsService _settings;

        private readonly IMapper _mapper;
        private readonly ActualCompanyDataService _actualCompanyDataService;
        private readonly LogsApi _logsApi;
        private readonly ServerLogSink _serverLog;
        private readonly MetricsReporter _metrics;

        /// <summary>
        ///     List of boards </summary>
        public List<Board> _boards;

        #endregion [ Private Fields ]

        #region [ Properties ]

        /// <summary>
        ///     Gets or sets view model </summary>
        public MainViewModel Model {
            get { return DataContext as MainViewModel; }
            set { DataContext = value; }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public MainWindow(MainManager manager, SettingsService settings, LoginManager loginManager, IMapper mapper,
                ActualCompanyDataService actualCompanyDataService, LogsApi logsApi, ServerLogSink serverLog, MetricsReporter metrics,
                EngineManager engine) {
            InitializeComponent();
            engine.SessionExpired += OnSessionExpired;
            _manager = manager;
            _manager.Initialize();
            _manager.InitializeCallServices();
            _manager.InitializeCallService(settings.GetCallServiceSettings());

            _settings = settings;
            _mapper = mapper;
            _actualCompanyDataService = actualCompanyDataService;
            _logsApi = logsApi;
            _serverLog = serverLog;
            _metrics = metrics;

            Title = loginManager.GetWindowTitle(Title);

            Loaded += MainWindow_Loaded;

#if !DEBUG
            // аудит D20: кнопка разработчика (закрывает софтфон) — только в Debug-сборке; V1 — Admin и apbestamerican@gmail.com
            btnDebug.Visibility = Visibility.Collapsed;
#endif
        }

        /// <summary>
        ///     Сервер ответил 401 — токен истёк (JWT живёт 30 дней) или пользователь удалён: окно входа поверх главного окна.
        ///     Тот же пользователь вошёл — работа продолжается с новым токеном, очередь запросов сохраняется; иначе App закрывается
        ///     (борды загружены под прежнего пользователя) </summary>
        private Task<bool> OnSessionExpired() {
            return Dispatcher.InvokeAsync(() => {
                var userId = UserSession.Current.UserId;
                _l.w("Session expired: sign in again");
                MessageBoxEx.Warning(this, "Your session has expired. Please sign in again.");
                var dialog = new LoginDialog(App.Services.GetRequiredService<LoginService>(), _settings) { Owner = this };
                if (dialog.ShowDialog() == true && UserSession.Current.UserId == userId) {
                    _l.i("Session expired: signed in again");
                    return true;
                }
                if (UserSession.Current.UserId != userId) {
                    _l.w("Session expired: signed in as a different user, closing");
                    MessageBoxEx.Info(this, "Signed in as a different user. Please start the application again.");
                    // раскладка и выбранные компании прежнего пользователя не пишутся в настройки вошедшего
                    _skipSavingColumns = true;
                } else {
                    _l.w("Session expired: sign-in cancelled, closing");
                }
                Close();
                return false;
            }).Task;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            // после входа есть токен: логи Information+ и метрики App уходят на сервер (как AutoCall V3)
            _serverLog.Attach(_logsApi);
            _metrics.Start();

            Initialize();
            InitializeBoards();
        }

        /// <summary>
        ///     Loading boards settings </summary>
        private void RadDocking_CustomElementLoading(object sender, LayoutSerializationCustomLoadingEventArgs e) {
            var board = _boards.FirstOrDefault(b => b.Title.Equals(e.ElementProperties["Title"]));
            if (board != null)
                //board.Parent
                e.SetAffectedElement(board);
        }

        #endregion [ .ctor ]

        #region [ Private Methods ]

        /// <summary>
        ///     Initialize view model </summary>
        private void Initialize() {
            var session = UserSession.Current;
            // Get board settings
            var boardSettings = _settings.GetBoardsSettings2()?.ToDictionary(e => e.Key, e => BoardSettingsConverter.ToVm(e.Value));
            if (boardSettings == null) {
                boardSettings = new Dictionary<BoardType, BoardSettingsViewModel>();
            }
            // Inserts missing modules
            foreach (BoardType boardType in Enum.GetValues(typeof(BoardType))) {
                if (boardSettings.ContainsKey(boardType) == false) {
                    boardSettings[boardType] = new BoardSettingsViewModel();
                }
            }

            // V1: цикл по модулям сессии (AutoDispatch, AutoCall) — в App модуль один, AutoDispatch
            Dictionary<BoardType, BoardSettingsViewModel> availableBoardSettings = new Dictionary<BoardType, BoardSettingsViewModel>();
            if (((RoleName)session.Roles).CheckPermission(RolesOperator.CarrierBoards)) {
                availableBoardSettings[BoardType.DispatchCarrier] = boardSettings[BoardType.DispatchCarrier];
                availableBoardSettings[BoardType.DispatchCarrierHistory] = boardSettings[BoardType.DispatchCarrierHistory];
                carrierBorder.Visibility = Visibility.Visible;
                bCarrierHistory.Visibility = Visibility.Visible;
            }
            if (((RoleName)session.Roles).CheckPermission(RolesOperator.BrokerBoards)) {
                availableBoardSettings[BoardType.DispatchBroker] = boardSettings[BoardType.DispatchBroker];
                brokerBorder.Visibility = Visibility.Visible;
            }
            if (((RoleName)session.Roles).CheckPermission(RolesOperator.AccountingBoards)) {
                availableBoardSettings[BoardType.DispatchBrokerAccount] = boardSettings[BoardType.DispatchBrokerAccount];
                availableBoardSettings[BoardType.DispatchCarrierAccount] = boardSettings[BoardType.DispatchCarrierAccount];
                bBrokerAccounting.Visibility = Visibility.Visible;
                bCarrierAccounting.Visibility = Visibility.Visible;
            } else {
                bBroker.Width = 95;
                bCarrier.Width = 95;
            }

            // Set view model to data context
            // V1: ViewType, Volume, VolumeMute, AllowHold — настройки модуля AutoCall (борды DatPower, звук уведомлений)
            Model = new MainViewModel() {
                BoardsSettings = availableBoardSettings,
            };
        }

        /// <summary>
        ///     Initializes boards </summary>
        private async void InitializeBoards() {
            // Go to "Loading" mode
            // NOTES: busy window
            Model.IsLoading = true;

            // Initialize all boards
            _boards = new List<Board>();

            // Creates settings view model
            BaseBoardViewModel._gridsSettings = new GridsSettingsViewModel();

            // Add boards control to window
            foreach (KeyValuePair<BoardType, BoardSettingsViewModel> boardSettings in Model.BoardsSettings) {
                Board board;
                switch (boardSettings.Key) {
                    case BoardType.DispatchCarrier:
                        board = new DispatchCarrierBoard(ModuleName.AutoDispatch, BoardType.DispatchCarrier, _manager, _settings);
                        break;
                    case BoardType.DispatchBroker:
                        board = new DispatchBrokerBoard(ModuleName.AutoDispatch, BoardType.DispatchBroker, _manager, _settings);
                        break;
                    case BoardType.DispatchCarrierAccount:
                        board = new DispatchCarrierAccountBoard(ModuleName.AutoDispatch, BoardType.DispatchCarrierAccount, _manager, _settings);
                        break;
                    case BoardType.DispatchBrokerAccount:
                        board = new DispatchBrokerAccountBoard(ModuleName.AutoDispatch, BoardType.DispatchBrokerAccount, _manager, _settings);
                        break;
                    case BoardType.DispatchCarrierHistory:
                        board = new DispatchCarrierHistoryBoard(ModuleName.AutoDispatch, BoardType.DispatchCarrierHistory, _manager, _settings);
                        break;
                    default:
                        throw new Exception();
                }
                _boards.Add(board);

                var binding = new Binding($"Enabled") {
                    Source = boardSettings.Value,
                    Converter = (IValueConverter)Resources["InverseBooleanConverter"],
                    Mode = BindingMode.TwoWay
                };
                board.SetBinding(Board.IsHiddenProperty, binding);

                BindingOperations.SetBinding(board.ScaleTransform, ScaleTransform.ScaleXProperty, new Binding("ZoomValue") { Source = boardSettings.Value, Mode = BindingMode.OneWay });
                BindingOperations.SetBinding(board.ScaleTransform, ScaleTransform.ScaleYProperty, new Binding("ZoomValue") { Source = boardSettings.Value, Mode = BindingMode.OneWay });
                board.ScaleSlider.SetBinding(Slider.ValueProperty, new Binding("ZoomValue") { Source = boardSettings.Value, Mode = BindingMode.TwoWay });

                // TODO NEED TO FIX STYLE
                //board.Style = (Style)Resources["BoardStyle"];
                DocumentGroup.AddItem(board, DockPosition.Center);

                board.InitializeGrids();
            }

            var session = UserSession.Current;

            #region ********************* Init dispatch board *********************

            // Gets list of actual companies
            var result = await _actualCompanyDataService.SearchActualCompaniesAsync(new SearchActualCompaniesRequest());
            if (result == null) {
                MessageBoxEx.Error(this, "Dispatch Company was not found");
                Environment.Exit(0);
            }
            if (result.IsSuccessed == false) {
                MessageBoxEx.Error(this, result.ErrorMessage);
                Environment.Exit(0);
            }
            var viewModels = result.ActualCompanies?.Select(ActualCompanyConverter.ToVm).ToList() ?? new List<ActualCompanyViewModel>();
            if (viewModels.Any(e => String.IsNullOrEmpty(e.SMTP_SchemeEmail) == false)) {
                btnScanScheme.Visibility = Visibility.Visible;
                btnScanBill.Visibility = Visibility.Visible;
            }

            Model.BrokerCompanies = viewModels.Where(c => c.BoardType == DispatchBoardType.Broker).Cast<object>().ToList();
            Model.CarrierCompanies = viewModels.Where(c => c.BoardType == DispatchBoardType.Carrier).Cast<object>().ToList();

            DispatchBoardViewModel.Instance.SetActualCompanies(viewModels);
            ((DispatchCarrierBoard)_boards.FirstOrDefault(b => b.BoardType == BoardType.DispatchCarrier))?.Initialize();
            ((DispatchBrokerBoard)_boards.FirstOrDefault(b => b.BoardType == BoardType.DispatchBroker))?.Initialize();
            ((DispatchCarrierAccountBoard)_boards.FirstOrDefault(b => b.BoardType == BoardType.DispatchCarrierAccount))?.Initialize();
            ((DispatchBrokerAccountBoard)_boards.FirstOrDefault(b => b.BoardType == BoardType.DispatchBrokerAccount))?.Initialize();
            ((DispatchCarrierHistoryBoard)_boards.FirstOrDefault(b => b.BoardType == BoardType.DispatchCarrierHistory))?.Initialize();

            // Gets Selected company settings
            if (Model.BrokerCompanies.Count() == 1) {
                SelectBrokerCompany((ActualCompanyViewModel)Model.BrokerCompanies.First());
            } else {
                SelectBrokerCompany((ActualCompanyViewModel)
                    Model.BrokerCompanies.FirstOrDefault(c => ((ActualCompanyViewModel)c).EntityId == _settings.GetSelectedBrokerCompany()));
            }
            if (Model.CarrierCompanies.Count() == 1) {
                SelectCarrierCompany((ActualCompanyViewModel)Model.CarrierCompanies.First());
            } else {
                SelectCarrierCompany((ActualCompanyViewModel)
                    Model.CarrierCompanies.FirstOrDefault(c => ((ActualCompanyViewModel)c).EntityId == _settings.GetSelectedCarrierCompany()));
            }

            // аудит D19: V1 показывал итоги только компании D2FD7AFF-… (неактивна с 2025-08, не переносится)
            canvasTotal.Visibility = Visibility.Collapsed;

            #endregion ********************* Init dispatch board *********************

            // NOTES: busy window
            Model.IsLoading = false;

            _settings.SaveSettings();

            // Call events (V1 CallChanged — статусы звонков у записей бордов DatPower, в App не нужен;
            // V1 CallRinging — панель входящего звонка, сигнал давал только BriaCallService, не перенесён — удалена, аудит E2)
            _manager.PhoneStatusChanged += OnPhoneStatusChanged;
            _manager.OpenDetails2 = OpenDetailsHandler;

            // Go back from "loading mode"
#if DEBUG
            ActivePane();
#endif
        }

        /// <summary>
        ///     Hang Up handler </summary>
        private async void Hangup_Clicked(object sender, RoutedEventArgs e) {
            await this._manager.GetCallService.HangCallAsync();
            // V1: сброс CallStatus у паттернов и записей бордов DatPower

            // принудительно дизаблить
            bCallingStatus.IsEnabled = false;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            bCallingStatus.IsEnabled = false;
        }

        private bool _сlosed = false;

        /// <summary>
        ///     On main window closing </summary>
        private async void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs args) {
            if (_сlosed == false) {
                _l.Stop();
                MicroSIPHelper.TryClose();
                AbtoPhoneHelper.TryClose();

                args.Cancel = true;
                // Shutdown in 10sec
#pragma warning disable 4014
                Task.Factory.StartNew(() => {
#pragma warning restore 4014
                    Thread.Sleep(10000);
                    Environment.Exit(0);
                });

                if (_skipSavingColumns == false) {
                    // Saves board settings
                    var boardSettings = Model.BoardsSettings.ToDictionary(e => e.Key, e => BoardSettingsConverter.ToSettings(e.Value));
                    foreach (Board board in _boards) {
                        var data = board.GetSettings();
                        boardSettings[board.BoardType].PatternColumnSettings2 = _resetColumns ? null : data.PatternColumnSettings2;
                        boardSettings[board.BoardType].RecordColumnSettings2 = _resetColumns ? null : data.RecordColumnSettings2;
                    }
                    _settings.SetBoardSettings2(boardSettings);
                    // V1 сохранял раскладку панелей (gridBoards.SaveLayout → DockingLayout), но загрузку закомментировал («sometimes panels
                    // will hidden») — раскладка не восстанавливалась, запись на сервер убрана (аудит E2)
                    // Saves selected companies
                    _settings.SetSelectedBrokerCompany(((ActualCompanyViewModel)Model.SelectedBrokerCompany)?.EntityId);
                    _settings.SetSelectedCarrierCompany(((ActualCompanyViewModel)Model.SelectedCarrierCompany)?.EntityId);
                }
                // V1: SaveSettings (WCF, синхронно) + Logout из API; у JWT logout не нужен — ждём запись изменённых секций
                await _settings.SaveSettingsAsync();
                _metrics.Dispose();
                _сlosed = true;
                Close();
            }
        }

        private bool _resetColumns;
        private bool _skipSavingColumns;

        private void btnResetColumns_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("The settings of the columns will be reseted\nThe application will be closed\nPlease confirm the operation", "Reset Settings", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK) {
                // V1 сначала брал дефолтные колонки и фильтры компании с сервера (GetDefaultColumnSettings/GetDefaultFilterSettings);
                // дефолты компании не перенесены (решено 2026-11-17, AI/wiki/user-settings.md) — сброс обычным способом, как V1 без дефолтов
                _resetColumns = true;
                MessageBox.Show("Settings was updated. Please restart application", "Restarting", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                Close();
            }
        }

        private void Map_Onclicked(object sender, RoutedEventArgs e) {
            // V1: MapRouteDialog без точек (Dispatch/RouteLocation2) — пустая карта /Map сервера
            _ = MapRouteHelper.OpenAsync();
        }

        /// <summary>
        ///     Настройки софтфона (вкладка Call Services) </summary>
        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            var dlg = new SettingsDialog(_manager);
            dlg.ShowOne();
            dlg.OnSaveSettings += SettingsDialog_Saved;
        }

        /// <summary>
        ///     On Saved Setting dialog </summary>
        private void SettingsDialog_Saved(object sender, EventArgs e) {
            Hangup_Clicked(sender, new RoutedEventArgs());
            _settings.SaveSettings();
            ((SettingsDialog)sender).OnSaveSettings -= SettingsDialog_Saved;
        }

        /// <summary>
        ///     Show debug info </summary>
        private async void btnDebug_Click(object sender, RoutedEventArgs args) {
            AbtoPhoneHelper.TryClose();
        }

        private void btnExpand_Click(object sender, RoutedEventArgs e) {
            Model.Expanded = !Model.Expanded;
        }

        /// <summary>
        ///     Shows hiding buttons </summary>
        private void Window_OnKeyDown(object sender, KeyEventArgs e) {
            foreach (var board in this._boards) {
                board.Board_OnKeyDown(sender, e);
            }
        }

        private void btnScanScheme_Click(object sender, RoutedEventArgs args) {
            var company = this.Model.BrokerCompanies.Union(this.Model.CarrierCompanies)
                .Cast<ActualCompanyViewModel>()
                .FirstOrDefault(e => String.IsNullOrEmpty(e.SMTP_SchemeEmail) == false);
            if (company != null) {
                var dlg = new ScanSchemeSendDialog(company);
                dlg.ShowDialog();
            }
        }

        private void btnScanBill_Click(object sender, RoutedEventArgs args) {
            var company = this.Model.BrokerCompanies.Union(this.Model.CarrierCompanies)
                .Cast<ActualCompanyViewModel>()
                .FirstOrDefault(e => String.IsNullOrEmpty(e.SMTP_SchemeEmail) == false);
            if (company != null) {
                var dlg = new ScannerDialog();
                dlg.Model.OkContent = "Open in Paint";
                if (dlg.ShowDialog() == true) {
                    foreach (var image in dlg.Model.Images) {
                        var binary = dlg.ImageToBinary(image);
                        var fileName = Path.GetTempFileName();
                        File.WriteAllBytes(fileName, binary);
                        // .NET: UseShellExecute по умолчанию false (AI/wiki/plan.md «Сканер TWAIN в App»)
                        Process.Start(new ProcessStartInfo("mspaint", fileName) { UseShellExecute = true });
                    }
                }
            }
        }

        #region [ Call events ]

        private void OnPhoneStatusChanged(bool calling) {
            bCallingStatus.IsEnabled = calling;
        }

        /// <summary>
        ///     Кнопка Details AutoPhone v.2. V1 открывал карточку груза DAT в борде DatPower Post Trucks (модуль AutoCall) — мёртвый код в App </summary>
        private async void OpenDetailsHandler(string loadId) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"OpenDetailsHandler");
                await Dispatcher.InvokeAsync(() => OpenDetailsHandler(loadId));
                return;
            }
            //_datPowerBoards.First(e => e.BoardType == BoardType.DatPowerPostTrucks).OpenDetails(loadId);
        }

        #endregion [ Call events ]

        #region [ Select actual company ]

        private void BrokerCompany_OnClicked(object sender, SelectionChangedEventArgs e) {
            SelectBrokerCompany((ActualCompanyViewModel)((RadComboBox)sender).SelectedItem);
        }

        private void CarrierCompany_OnClicked(object sender, SelectionChangedEventArgs e) {
            SelectCarrierCompany((ActualCompanyViewModel)((RadComboBox)sender).SelectedItem);
        }

        private void SelectBrokerCompany(ActualCompanyViewModel company) {
            Model.SelectedBrokerCompany = company;
            foreach (Board board in _boards) {
                board.SelectedBrokerCompany(company);
            }
        }

        private void SelectCarrierCompany(ActualCompanyViewModel company) {
            Model.SelectedCarrierCompany = company;
            foreach (Board board in _boards) {
                board.SelectedCarrierCompany(company);
            }
        }

        #endregion [ Select actual company ]

        #endregion [ Private Methods ]
    }
}
