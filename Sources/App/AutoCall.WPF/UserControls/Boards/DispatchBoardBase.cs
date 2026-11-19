using AutoCall.Common;
using AutoDispatch.Logic.Converters;
using AutoCall.Common.Attributes;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Common.Helpers;
using AutoCall.Common.Loggers;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Settings.Sections;
using AutoCall.WPF.Converters;
using AutoCall.WPF.Converters.Dispatch;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Managers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using AutoCall.Common.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using static AutoCall.WPF.Converters.Dispatch.DispatchBoard_BoardToCommentNameConverter;
using DispatchHelper = AutoDispatch.Logic.Helpers.DispatchHelper;
using ResponseStatus = AutoDispatch.Dtos.Enums.ResponseStatus;

namespace AutoCall.WPF.UserControls.Boards {
    public abstract class DispatchBoardBase : Board, IDispatchControl {

        #region [ Protected Fields ]

        /// <summary>
        ///     Engine manager </summary>
        protected static EngineManager _engine;
        /// <summary>
        ///     Dispatch service instance </summary>
        protected static DispatchService _dispatchService;
        /// <summary>
        ///     AutoMapper instance </summary>
        protected static IMapper _mapper;
        /// <summary>
        ///     AutoCall manager instance </summary>
        protected MainManager _manager;
        /// <summary>
        ///     Use Session </summary>
        protected static AutoDispatch.Data.UserSession _session;

        private static DispatchBoardViewModel _model;

        private static bool _loadsInitCompleted;
        private static bool _warehousesInitCompleted;
        private static bool _yardsInitCompleted;
        private static bool _trailersInitCompleted;
        private static bool _trucksInitCompleted;
        private static bool _driversInitCompleted;
        private static bool _datCompaniesInitCompleted;
        private static bool _datDriversInitCompleted;
        private static bool _setupPacketsInitCompleted;
        private static bool _totalCarrierHistoryInitCompleted;
        private static bool _totalBrokerHistoryInitCompleted;

        #endregion [ Protected Fields ]

        #region [ Properties ]

        public abstract DispatchBoardType DispatchBoardType { get; }

        /// <summary>
        ///     Конвервер DispatchBoard_BoardToCommentNameConverter пересталд работать </summary>
        protected string GetCommentHeaderText(CommentType commentType) {
            switch (commentType) {
                case CommentType.Comment1:
                    switch (this.DispatchBoardType) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Comments for Dispatcher";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "External Comments";
                        default:
                            throw new Exception();
                    }
                case CommentType.Comment2:
                    switch (this.DispatchBoardType) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Comments for Broker";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "Internal Comments";
                        default:
                            throw new Exception();
                    }
                case CommentType.Consolidation:
                    switch (this.DispatchBoardType) {
                        case DispatchBoardType.CarrierHistory:
                        case DispatchBoardType.Carrier:
                        case DispatchBoardType.CarrierAccount:
                            return "Consolidation";
                        case DispatchBoardType.Broker:
                        case DispatchBoardType.BrokerAccount:
                            return "Consolidation";
                        default:
                            throw new Exception();
                    }
                default:
                    throw new Exception();
            }
        }

        ActualCompanyViewModel IDispatchControl.SelectedBrokerCompany => Model.SelectedBrokerCompany;

        ActualCompanyViewModel IDispatchControl.SelectedCarrierCompany => Model.SelectedCarrierCompany;

        /// <summary>
        ///     Gets or sets board view model </summary>
        public DispatchBoardViewModel Model {
            get { return _model; }
            set { _model = value; }
        }

        public DispatchBoardSelectedItemsViewModel SelectedItemsModel { get; set; }

        public DispatchActions DispatchActions { get; }
        public DispatchCommands DispatchCommands { get; }
        // abbr
        public DispatchCommands dc => DispatchCommands;

        protected abstract LoadingControl loading1 { get; }

        #endregion [ Properties ]

        #region [.ctor ]

        static DispatchBoardBase() {
            // one model for all boards
            _model = new DispatchBoardViewModel();
            _session = UserSession.Current;
        }

        public DispatchBoardBase(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings)
            : base(moduleName, boardType, settings) {
            _manager = manager;

            SelectedItemsModel = new DispatchBoardSelectedItemsViewModel();

            _dispatchService = App.Services.GetRequiredService<DispatchService>();
            _engine = App.Services.GetRequiredService<EngineManager>();
            _mapper = App.Services.GetRequiredService<IMapper>();

            DispatchActions = new DispatchActions(this);
            DispatchCommands = new DispatchCommands(DispatchActions, this);
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        protected void CreateGainAndExpenseColumns(RadGridView gridView, bool linkedLoad = false) {
            var prefixBinding = linkedLoad == true ? "LinkedWrapper." : String.Empty;
            var prefixHeader = linkedLoad == true ? "*" : String.Empty;
            var prefixGroupName = linkedLoad == true ? "Linked Load → " : String.Empty;
            var expenses = Enum.GetValues(typeof(ExpenseType))
                .Cast<ExpenseType>()
                .Where(x => DisplayAttribute.HasDisplay(x))
                .OrderBy(x => DisplayAttribute.GetOrder(x))
                .ToList();
            for (var i = 1; i < expenses.Count; i++) {
                ExpenseType expense = expenses[i];

                // a custom .ctor
                GridViewExpensesColumn column1 = new GridViewExpensesColumn(gridView, linkedLoad, i, expense);
                gridView.Columns.Add(column1);
                column1.Header = $"{prefixHeader}{expense.GetDescription()}";
                column1.DataMemberBinding = new Binding($"{prefixBinding}Load.ExpensesView[{i}].Cost") { Converter = new CurrencyFormatConverter() };
                column1.IsReadOnly = false;
                column1.ColumnGroupName = $"{prefixBinding}Expenses";
                column1.Width = column1.MinWidth = 60;
                column1.IsVisible = false;
                // # slow. this function calculate by GainsView
                //column1.AggregateFunctions.Add(new SumFunction() { ResultFormatString = "{0:C2}" });
                PropertyExtensions.SetRelativeDisplayIndex(column1, i * 2);
                PropertyExtensions.SetRelativeGroup(column1, column1.ColumnGroupName + i);

                var column2 = gridView.CreateTextColumn($"{prefixHeader}{expense.GetDescription()} Comment", $"{prefixBinding}Load.ExpensesView[{i}].Comment_Date");
                column2.IsReadOnly = false;
                column2.Tag = expense;
                column2.CellStyle = new Style(typeof(GridViewCell), (Style)gridView.Resources[typeof(GridViewCell)]);
                column2.SetColumnStyle1($"{prefixBinding}Load.ExpensesView[{i}].Paid", $"{prefixBinding}Load.Parent1");
                column2.SortMemberPath = $"{prefixBinding}Load.ExpensesView[{i}].PaidDate";
                column2.ColumnGroupName = $"{prefixBinding}Expenses";
                column2.Width = column2.MinWidth = 60;
                column2.IsVisible = false;
                PropertyExtensions.SetRelativeDisplayIndex(column2, i * 2 + 1);
                PropertyExtensions.SetRelativeGroup(column2, column2.ColumnGroupName + i);
            }

            var gains = Enum.GetValues(typeof(GainType))
                .Cast<GainType>()
                .Where(x => DisplayAttribute.HasDisplay(x))
                .OrderBy(x => DisplayAttribute.GetOrder(x))
                .ToList();
            for (var i = 1; i < gains.Count; i++) {
                GainType gain = gains[i];

                // a custom .ctor
                GridViewGainsColumn column1 = new GridViewGainsColumn(gridView, linkedLoad, i, gain);
                gridView.Columns.Add(column1);
                column1.Header = $"{prefixHeader}{gain.GetDescription()}";
                column1.DataMemberBinding = new Binding($"{prefixBinding}Load.GainsView[{i}].Cost") { Converter = new CurrencyFormatConverter() };
                column1.IsReadOnly = false;
                column1.ShowFieldFilters = false;
                column1.ColumnGroupName = $"{prefixBinding}Accessorial";
                column1.Width = column1.MinWidth = 60;
                column1.IsVisible = false;
                // # slow. this function calculate by GainsView
                //column1.AggregateFunctions.Add(new SumFunction() { ResultFormatString = "{0:C2}" });
                PropertyExtensions.SetRelativeDisplayIndex(column1, i * 2);
                PropertyExtensions.SetRelativeGroup(column1, column1.ColumnGroupName + i);

                var column2 = gridView.CreateTextColumn($"{prefixHeader}{gain.GetDescription()} Comment", $"{prefixBinding}Load.GainsView[{i}].Comment_Date");
                column2.IsReadOnly = false;
                column2.Tag = gain;
                column2.CellStyle = new Style(typeof(GridViewCell), (Style)gridView.Resources[typeof(GridViewCell)]);
                column2.SetColumnStyle1($"{prefixBinding}Load.GainsView[{i}].Paid", $"{prefixBinding}Load.Parent1");
                column2.SortMemberPath = $"{prefixBinding}Load.GainsView[{i}].PaidDate";
                column2.ColumnGroupName = $"{prefixBinding}Accessorial";
                column2.Width = column2.MinWidth = 60;
                column2.IsVisible = false;
                PropertyExtensions.SetRelativeDisplayIndex(column2, i * 2 + 1);
                PropertyExtensions.SetRelativeGroup(column2, column2.ColumnGroupName + i);
            }
            gridView.ColumnGroups.Add(new GridViewColumnGroup() { Header = $"{prefixGroupName}Accessorial", Name = $"{prefixBinding}Accessorial" });
            gridView.ColumnGroups.Add(new GridViewColumnGroup() { Header = $"{prefixGroupName}Expenses", Name = $"{prefixBinding}Expenses" });
        }

        private void StaticUninitialize() {
            Model.Clear();

            _engine.EngineQueueCountChanged -= _engine_QueueCountChanged;
            _engine.EngineStateChanged -= _engine_EngineStateChanged;
            _engine.EngineTryRequest -= _engine_EngineTryRequest;
            _engine.EngineRequestEnd -= _engine_EngineRequestEnd;
            _engine.EventReceived -= _engine_EventReceived;

            _dispatchService.LoadsReceivedAsync -= _service_LoadsReceivedAsync;
            _dispatchService.DriversReceivedAsync -= _service_DriversReceivedAsync;
            _dispatchService.TrucksReceivedAsync -= _service_TrucksReceivedAsync;
            _dispatchService.TrailersReceivedAsync -= _service_TrailersReceivedAsync;
            _dispatchService.WarehousesReceivedAsync -= _service_WarehousesReceivedAsync;
            _dispatchService.DatCompaniesReceivedAsync -= _service_DatCompaniesReceivedAsync;
            _dispatchService.DatDriversReceivedAsync -= _service_DatDriversReceivedAsync;
            _dispatchService.SetupPacketsReceivedAsync -= _service_SetupPacketsReceivedAsync;

            GC.Collect();
        }

        private static bool _initialized = false;
        /// <summary>
        ///     Initialize board </summary>
        public void StaticInitialize() {

            _engine.EngineQueueCountChanged += _engine_QueueCountChanged;
            _engine.EngineStateChanged += _engine_EngineStateChanged;
            _engine.EngineTryRequest += _engine_EngineTryRequest;
            _engine.EngineRequestEnd += _engine_EngineRequestEnd;
            _engine.EventReceived += _engine_EventReceived;

            if (_initialized == true) {
                return;
            }
            _initialized = true;

            _engine.StartResponse(
                new EventTypeDto[] {
                    EventTypeDto.BookedLoad,
                    EventTypeDto.Driver,
                    EventTypeDto.Truck,
                    EventTypeDto.Trailer,
                    EventTypeDto.Warehouse,
                    EventTypeDto.DatCompany,
                    EventTypeDto.DatDriver,
                    EventTypeDto.SetupPacket,
                    EventTypeDto.HistoryBookedLoad,
                    EventTypeDto.HistoryDriver,
                    EventTypeDto.HistoryTruck,
                    EventTypeDto.HistoryTrailer,
                    EventTypeDto.HistoryDatDriver,
                });

            RefreshStatistics();

            _dispatchService.LoadWarehouses(LoadWarehouses_Completed);
            _dispatchService.LoadYards(LoadYards_Completed);
            _dispatchService.LoadTrailers(LoadTrailers_Completed);
            _dispatchService.LoadTrucks(LoadTrucks_Completed);
            _dispatchService.LoadDrivers(LoadDrivers_Completed);
            _dispatchService.LoadDatCompanies(LoadDatCompanies_Completed);
            _dispatchService.LoadDatDrivers(LoadDatDrivers_Completed);
            _dispatchService.LoadSetupPackets(LoadSetupPackets_Completed);
            _dispatchService.LoadLoads(LoadLoads_Completed);
            _dispatchService.LoadTotalHistoryLoads(DispatchBoardType.CarrierAccount, LoadCarrierTotalHistoryLoads_Completed);
            _dispatchService.LoadTotalHistoryLoads(DispatchBoardType.BrokerAccount, LoadBrokerTotalHistoryLoads_Completed);

            _dispatchService.LoadsReceivedAsync += _service_LoadsReceivedAsync;
            _dispatchService.DriversReceivedAsync += _service_DriversReceivedAsync;
            _dispatchService.TrucksReceivedAsync += _service_TrucksReceivedAsync;
            _dispatchService.TrailersReceivedAsync += _service_TrailersReceivedAsync;
            _dispatchService.WarehousesReceivedAsync += _service_WarehousesReceivedAsync;
            _dispatchService.YardsReceivedAsync += _service_YardsReceivedAsync;
            _dispatchService.DatCompaniesReceivedAsync += _service_DatCompaniesReceivedAsync;
            _dispatchService.DatDriversReceivedAsync += _service_DatDriversReceivedAsync;
            _dispatchService.SetupPacketsReceivedAsync += _service_SetupPacketsReceivedAsync;

            // Loading Filters
            foreach (DispatchFilterTableType filterType in Enum.GetValues(typeof(DispatchFilterTableType))) {
                FiltersColumnsSettings fitlersColumnsSettigns = _settings.GetFiltersColumns((byte)filterType);
                if (fitlersColumnsSettigns != null) {
                    var fitlersColumnsViewModel = FiltersColumnsConverter.ToVm(fitlersColumnsSettigns);
                    Model.SetFiltersColumns(filterType, fitlersColumnsViewModel);
                } else {
                    Model.SetFiltersColumns(filterType, new FiltersColumnsViewModel() { Table = filterType });
                }
            }

            // Loading States to Zones (V1 — справочники State/Zone WebV1 через IStateRepository/IZoneRepository)
            FilterHelper.StateToZone = StateZones.StateToZone;

            if (_session.CustomStatuses != null) {
                foreach (var keyValue in _session.CustomStatuses) {
                    Model.CustomStatuses[keyValue.Key] = keyValue.Value;
                }
            }
        }

        protected void CreateFilterBinding(FilterColumnsControl itemsControl, DispatchFilterTableType table) {
            {
                var binding = new Binding();
                binding.Path = new PropertyPath(nameof(Model.FiltersColumns));
                binding.Converter = new DictionaryToValueConverter();
                binding.ConverterParameter = table;
                binding.Mode = BindingMode.OneWay;
                itemsControl.SetBinding(ItemsControl.DataContextProperty, binding);
            }
            {
                var binding = new Binding();
                binding.Path = new PropertyPath(nameof(FiltersColumnsViewModel.Filters));
                binding.Mode = BindingMode.OneWay;
                itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, binding);
            }
            if (table == DispatchFilterTableType.Broker_Patterns || table == DispatchFilterTableType.Carrier_Patterns) {
                itemsControl.SelectionChanged = Patterns_FilterColumns_SelectionChanged;
            } else {
                itemsControl.SelectionChanged = Records_FilterColumns_SelectionChanged;
            }
        }

        protected abstract void Patterns_FilterColumns_SelectionChanged();
        protected abstract void Records_FilterColumns_SelectionChanged();

        public void Initialize() {
            StaticInitialize();
        }

        #region helpers
        private bool checkSingle(object param) {
            return ((IList<object>)param)?.Count == 1;
        }
        private Wrapper1LoadViewModel getWrapper(object param) {
            return (Wrapper1LoadViewModel)((IList<object>)param).First();
        }
        private LoadViewModel getLoad(object param) {
            return getWrapper(param).Load;
        }
        #endregion helpers

        // event received
        private async Task OnCanExecuteChanged(EventResponse response) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"OnCanExecuteChanged");
                await this.Dispatcher.InvokeAsync(async () => await OnCanExecuteChanged(response));
                return;
            }
            if (DispatchCommands == null) {
                _l.f("#err ce654d80049d906e5e325a9782f8354fe");
            }
            DispatchCommands.OnCanExecuteChanged();
        }

        public override void InitializeGrids() {
            base.InitializeGrids();
            // refreshes all commands
            _engine.EventReceived -= OnCanExecuteChanged;
            _engine.EventReceived += OnCanExecuteChanged;

        }

        // refreshes menuitems IsEnabled -> visibility
        protected virtual void RadContextMenu_Opened(object sender, RoutedEventArgs e) {
            DispatchCommands.OnCanExecuteChanged();
        }

        #endregion [ Initialize ]

        #region [ Loaded ]

        private async static Task LoadLoads_Completed(ViewModelWrapper<List<LoadViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadLoads_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadLoads_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetLoadsAsync(wrapper.Data, refreshWrappers: false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _loadsInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadWarehouses_Completed(ViewModelWrapper<List<WarehouseViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadWarehouses_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadWarehouses_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetWarehousesAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _warehousesInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadYards_Completed(ViewModelWrapper<List<YardViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadYards_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadYards_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetYardsAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _yardsInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadTrailers_Completed(ViewModelWrapper<List<TrailerViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadTrailers_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadTrailers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetTrailersAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _trailersInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadTrucks_Completed(ViewModelWrapper<List<TruckViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadTrucks_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadTrucks_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetTrucksAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _trucksInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadDrivers_Completed(ViewModelWrapper<List<DriverViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadDrivers_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadDrivers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetDriversAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _driversInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadDatCompanies_Completed(ViewModelWrapper<List<DatCompanyViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadDatCompanies_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadDatCompanies_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetDatCompaniesAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _datCompaniesInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadDatDrivers_Completed(ViewModelWrapper<List<DatDriverViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadDatDrivers_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadDatDrivers_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetDatDriversAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _datDriversInitCompleted = true;
            CheckLoadCompleted();
        }

        private async static Task LoadSetupPackets_Completed(ViewModelWrapper<List<SetupPacketViewModel>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadSetupPackets_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadSetupPackets_Completed(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                await _model.SetSetupPacketsAsync(wrapper.Data, false);
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
            _setupPacketsInitCompleted = true;
            CheckLoadCompleted();
        }

        protected async static Task LoadCarrierTotalHistoryLoads_Completed(ViewModelWrapper<Dictionary<DateTime, int>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadCarrierTotalHistoryLoads_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadCarrierTotalHistoryLoads_Completed(wrapper));
                return;
            }
            await LoadTotalHistoryLoads_Completed(_model.CarrierHistoryLoads, wrapper);
            // TODO: implement observable dictionary
            //_model.CarrierHistoryLoadsOnChanged();
            _totalCarrierHistoryInitCompleted = true;
            CheckLoadCompleted();
        }

        protected async static Task LoadBrokerTotalHistoryLoads_Completed(ViewModelWrapper<Dictionary<DateTime, int>> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"LoadBrokerTotalHistoryLoads_Completed");
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadBrokerTotalHistoryLoads_Completed(wrapper));
                return;
            }
            await LoadTotalHistoryLoads_Completed(_model.BrokerHistoryLoads, wrapper);
            // TODO: implement observable dictionary
            //_model.BrokerHistoryLoadsOnChanged();
            _totalBrokerHistoryInitCompleted = true;
            CheckLoadCompleted();
        }

        private static async Task LoadTotalHistoryLoads_Completed(Dictionary<DateTime, int> historyLoads, ViewModelWrapper<Dictionary<DateTime, int>> wrapper) {
            if (wrapper.Status == ResponseStatus.Success) {
                historyLoads.Clear();
                foreach (KeyValuePair<DateTime, int> i in wrapper.Data.OrderByDescending(kv => kv.Key)) {
                    historyLoads.Add(i.Key, i.Value);
                }
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
        }

        /// <summary>
        ///     Check buzy indicator </summary>
        private static async void CheckLoadCompleted() {
            if (_loadsInitCompleted &&
                _driversInitCompleted &&
                _trucksInitCompleted &&
                _trailersInitCompleted &&
                _warehousesInitCompleted &&
                _yardsInitCompleted &&
                _setupPacketsInitCompleted &&
                _datCompaniesInitCompleted &&
                _datDriversInitCompleted &&
                _totalCarrierHistoryInitCompleted &&
                _totalBrokerHistoryInitCompleted) {
                await _model.RefreshWrappersAsync();
                _model.Loading = false;
                _model.LoadedInvoke();
                Debug.WriteLine("Loaded");
            }
        }

        #endregion [ Loaded ]

        /* Use .Dispatcher.Invoke instead BeginInvoke for ordering event handlers, LogicEvent as first, ViewEvent as last. */
        #region [ Received ]

        protected static Dictionary<DispatchFilterTableType, RadGridView> Grids = new Dictionary<DispatchFilterTableType, RadGridView>();

        private static async Task _service_LoadsReceivedAsync(IList<LoadViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_LoadsReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_LoadsReceivedAsync(viewModels));
                return;
            }
            // System.NullReferenceException: Object reference not set to an instance of an object.
            //    at AutoCall.WPF.UserControls.Boards.DispatchBoardBase.cdecd3578549503d271e2f003c9ac50b0(IList`1 c3d35c8a85bcd3b4bd6270e1cf22b5a33)
            if (_model == null) {
                _l.f("Model is null. #err");
            }
            //var loadToTrailer = GetLoadToTrailer(viewModels); // gets loadId -> board
            Dictionary<Guid, DispatchFilterLoadType> loadToBoard = GetLoadToBoard(viewModels.Select(e => e.EntityId).ToList());
            if (await _model.SetLoadsAsync(viewModels, isExpanded: true)) {
                RefreshStatistics();
            }
        }

        /// <summary>
        ///     Returns LoadId -> GridName </summary>
        private static Dictionary<Guid, DispatchFilterLoadType> GetLoadToBoard(List<Guid> loadIds) {
            var result = new Dictionary<Guid, DispatchFilterLoadType>();
            foreach (var id in loadIds) {
                Wrapper1LoadViewModel wrapper = DispatchBoardViewModel.Instance.Wrappers.ToList().FirstOrDefault(e => e.Load.EntityId == id);
                if (wrapper != null && wrapper.Load.ParentId != null) {
                    result[id] = Static_ViewSource_OnFilter(wrapper);
                }
            }
            return result;
        }

        private async static Task _service_TrucksReceivedAsync(IList<TruckViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_TrucksReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_TrucksReceivedAsync(viewModels));
                return;
            }
            await _model.SetTrucksAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_TrailersReceivedAsync(IList<TrailerViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_TrailersReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_TrailersReceivedAsync(viewModels));
                return;
            }
            await _model.SetTrailersAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_WarehousesReceivedAsync(IList<WarehouseViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_WarehousesReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_WarehousesReceivedAsync(viewModels));
                return;
            }
            await _model.SetWarehousesAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_YardsReceivedAsync(IList<YardViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_YardsReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_YardsReceivedAsync(viewModels));
                return;
            }
            await _model.SetYardsAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_DriversReceivedAsync(IList<DriverViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_DriversReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_DriversReceivedAsync(viewModels));
                return;
            }
            await _model.SetDriversAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_DatCompaniesReceivedAsync(IList<DatCompanyViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_DatCompaniesReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_DatCompaniesReceivedAsync(viewModels));
                return;
            }
            await _model.SetDatCompaniesAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_DatDriversReceivedAsync(IList<DatDriverViewModel> viewModels) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_DatDriversReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_DatDriversReceivedAsync(viewModels));
                return;
            }
            await _model.SetDatDriversAsync(viewModels);
            RefreshICollectionViews();
        }

        private async static Task _service_SetupPacketsReceivedAsync(IList<SetupPacketViewModel> setupPackets) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_service_SetupPacketsReceived");
                await Application.Current.Dispatcher.InvokeAsync(async () => await _service_SetupPacketsReceivedAsync(setupPackets));
                return;
            }
            await _model.SetSetupPacketsAsync(setupPackets);
            RefreshICollectionViews();
        }

        // TODO: доделай сохранение выделения. 
        // TODO: доделай выделение одного елемена во всех гридах
        private static void RefreshICollectionViews() {
            // за что отвечает этот код? 
            //var selections = new Dictionary<DispatchFilterTableType, List<Guid>>();

            //foreach (KeyValuePair<DispatchFilterTableType, RadGridView> keyValue in Grids) {
            //    try {
            //        // saves selection
            //        selections[keyValue.Key] = keyValue.Value.SelectedItems.Cast<Wrapper1LoadViewModel>().Select(e => e.Load.EntityId).ToList();
            //    } catch (Exception e) {
            //        _l.e("RefreshICollectionViews", e);
            //    }
            //}

            //foreach (KeyValuePair<DispatchFilterTableType, RadGridView> keyValue in Grids) {
            //    try {
            //        // refreshes collection
            //        ((ICollectionView)keyValue.Value.ItemsSource).Refresh();
            //        await Task.Delay(100);
            //        // restores selection
            //        keyValue.Value
            //            .Select(((IEnumerable)keyValue.Value.ItemsSource)
            //            .Cast<Wrapper1LoadViewModel>()
            //            .Where(e => selections[keyValue.Key].Contains(e.Load.EntityId)));
            //    } catch (Exception e) {
            //        _l.e("RefreshICollectionViews", e);
            //    }
            //}
        }

        private static void RefreshStatistics() {
            _dispatchService.GetStatistics(GetStatisticsCompeted);
        }

        private async static Task GetStatisticsCompeted(ViewModelWrapper<int> wrapper) {
            if (Application.Current.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"GetStatisticsCompeted");
                await Application.Current.Dispatcher.InvokeAsync(async () => await GetStatisticsCompeted(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                ((MainWindow)Application.Current.MainWindow).Model.Total = wrapper.Data;
            } else {
                _webl.e(wrapper.MessageError);
            }
        }

        #endregion [ Received ]

        #region [ Engine Indicator ]

        private TextBlock tbEngineQueueCount => (TextBlock)this.FindName("tbEngineQueueCount");

        protected async void _engine_EngineTryRequest(string methodName, long size, TimeSpan time) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_engine_EngineStateChanged");
                await Dispatcher.InvokeAsync(() => _engine_EngineTryRequest(methodName, size, time));
                return;
            }
            Model.EngineState = EngineState.Buzy;
        }

        protected async void _engine_EngineRequestEnd(string methodName, long size, TimeSpan time) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_engine_EngineStateChanged");
                await Dispatcher.InvokeAsync(() => _engine_EngineRequestEnd(methodName, size, time));
                return;
            }
            Model.EngineState = EngineState.Connected;
#if !DEBUG
            if (((RoleName)_session.Roles).CheckPermission(RoleName.Admin) == true)
#endif
            {
                var txt = new List<string>();
                if (size > 100000) {
                    txt.Add(StringExtension.ToSize(size));
                }
                if (time > TimeSpan.FromSeconds(1)) {
                    txt.Add($"{(int)time.TotalSeconds}s");
                }
                if (txt.Any()) {
                    tbEngineQueueCount.Text += $", {methodName} {String.Join("/", txt)}";
                    tbEngineQueueCount.Text = tbEngineQueueCount.Text.Reverse1().Truncate(140).Reverse1().TrimStart(' ', ',');
                }
            }
        }

        protected async void _engine_EngineStateChanged(ConnectionStatus status) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_engine_EngineStateChanged");
                await Dispatcher.InvokeAsync(() => _engine_EngineStateChanged(status));
                return;
            }
            if (status == ConnectionStatus.Connected)
                Model.EngineState = EngineState.Connected;
            if (status == ConnectionStatus.Disconected)
                Model.EngineState = EngineState.Disconected;
        }

        protected void _engine_QueueCountChanged(int queueCount) {
            Dispatcher.BeginInvoke((Action)(() => {
                //tbEngineQueueCount.Text = queueCount.ToString();
                //tbEngineQueueCount.Visibility = queueCount != 0 ? Visibility.Visible : Visibility.Hidden;
            }));
        }

        protected async virtual Task _engine_EventReceived(EventResponse response) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_engine_EventReceived");
                await Dispatcher.InvokeAsync(async () => await _engine_EventReceived(response));
                return;
            }
            Model.EngineState = EngineState.Connected;
        }

        #endregion [ Engine Indicator ]

        #region [ Busy Indicator ]

        public virtual void BusyStart(string message = null) {
            loading1.Visibility = Visibility.Visible;
            loading1.progressBar.IsIndeterminate = true;
            loading1.LoadingMessage = message;
        }

        public async virtual void BusyFinish() {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"_engine_EventReceived");
                await Dispatcher.InvokeAsync(this.BusyFinish);
                return;
            }
            loading1.Visibility = Visibility.Collapsed;
        }

        // "close" need to interface
        public async virtual Task SaveCompleted<T>(ViewModelWrapper<T> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"SaveCompleted");
                await Dispatcher.InvokeAsync(() => SaveCompleted(wrapper));
                return;
            }
            loading1.Visibility = Visibility.Collapsed;
            if (wrapper.Status == ResponseStatus.Success) {
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
        }

        // "close" need to interface
        public async virtual Task SaveCompleted<T, F>(ViewModelWrapper<T> wrapper1, ViewModelWrapper<F> wrapper2) {
            if (this.Dispatcher.CheckAccess() == false) {
                StackLogger.Push($"SaveCompleted");
                await Dispatcher.InvokeAsync(() => SaveCompleted(wrapper1, wrapper2));
                return;
            }
            this.loading1.Visibility = Visibility.Collapsed;
            if (wrapper1.Status == ResponseStatus.Success && wrapper2.Status == ResponseStatus.Success) {
                // Node: without 'Model.SetLoads'
            } else {
                MessageBoxEx.Error((wrapper1.MessageError ?? "") + "\n" + (wrapper2.MessageError ?? ""));
            }
        }

        #endregion [ Busy Indicator ]

        #region [ Selection ]

        // RefreshICollectionViews() - saving selection after event

        /// <summary>
        ///     DataGrid clear selection by click on space </summary>
        protected void grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            // V1: DatPowerBoard._stopTimer1 — простой постингов DAT (модуль AutoCall), не перенесён
            ExitAppTimer.Restart();

            if (e.OriginalSource is FrameworkElement || e.OriginalSource is FrameworkContentElement) {
                var element = (DependencyObject)e.OriginalSource;
                var dataContext = UIHelper.GetDataContext(e.OriginalSource);
                // it is not row and cell
                if (dataContext is Wrapper1LoadViewModel == false) {
                    if (dataContext is LoadViewModel == false) {
                        if (element.GetVisualParent<GridViewHeaderCell>() == null /* not column */ &&
                                (
                                 UIHelper.GetName(element) == "PART_RootPanel" ||
                                 UIHelper.GetName(element) == "PART_FooterCellBorder" ||
                                 element.GetVisualParent<CommonColumnHeader>() != null /* group header */ ||
                                 element.GetVisualParent<GridViewGroupPanel>() != null /* group panel */
                                )
                                ) {
                            try {
                                if (SelectedItemsModel.SelectedPatterns.Count > 0) {
                                    SelectedItemsModel.SelectedPatterns.Clear();
                                }
                                if (SelectedItemsModel.SelectedLoads.Count > 0) {
                                    SelectedItemsModel.SelectedLoads.Clear();
                                }
                            } catch {
                            }
                        }
                    }
                } else {
                    // NOTES: clear marker
                    ((Wrapper1LoadViewModel)dataContext).IsNew = false;
                }
            }
        }

        /// <summary>
        ///     Selects row </summary>
        protected void gridRow_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            ExitAppTimer.Restart();
            //e.Handled = true;
        }

        /// <summary>
        ///     Selects row </summary>
        protected void gridRow_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            // V1: DatPowerBoard._stopTimer1 — простой постингов DAT (модуль AutoCall), не перенесён
            ExitAppTimer.Restart();

            var row = (GridViewRowItem)sender;
            var dataGrid = ((GridViewRowItem)sender).GridViewDataControl;

            if (row.DataContext == null) {
                _l.f("row.DataContext == null #err");
                return;
            }
            if (SelectedItemsModel.SelectedPatterns == null) {
                _l.f("Model.SelectedPatterns == null #err");
                return;
            }
            if (SelectedItemsModel.SelectedLoads == null) {
                _l.f("Model.SelectedLoads == null #err");
                return;
            }

            // Checks if clicked by subload and eventhandler on parent
            if (dataGrid != null && e.Source is ContentPresenter == false) {
                dataGrid.Focus();
                try {
                    if (SelectedItemsModel.SelectedPatterns.Count > 0) {
                        SelectedItemsModel.SelectedPatterns.Clear();
                    }
                    if (SelectedItemsModel.SelectedLoads.Count > 0) {
                        SelectedItemsModel.SelectedLoads.Clear();
                    }
                    if (dataGrid.SelectedItems.Contains(row.DataContext) == false) {
                        dataGrid.SelectedItems.Add(row.DataContext);
                    }
                } catch (Exception) {
                }
                ((Wrapper1LoadViewModel)row.DataContext).IsNew = false;
            }
        }

        protected void grid_SelectionChanged(object sender, SelectionChangeEventArgs args) {
            var dataGrid = (RadGridView)sender;
            if (dataGrid.Name == "gridPatterns") {
                // Marks subloads
                foreach (Wrapper1LoadViewModel wrapper2 in DispatchBoardViewModel.Instance.Wrappers.ToList().Where(e => e.Load.ParentId != null)) {
                    wrapper2.Load.Mark = false;
                    foreach (Wrapper1LoadViewModel wrapper1 in dataGrid.SelectedItems.Cast<Wrapper1LoadViewModel>().Where(e => e.Load.ParentId != null)) {
                        wrapper2.Load.Mark = wrapper1 != wrapper2 && wrapper1.Load.ParentId == wrapper2.Load.ParentId;
                    }
                }
            }
            if (dataGrid.Name == "gridLoads") {
                // Marks subloads
                foreach (Wrapper1LoadViewModel wrapper2 in DispatchBoardViewModel.Instance.Wrappers.ToList().Where(e => e.Load.ParentId != null)) {
                    wrapper2.Load.Mark = false;
                    foreach (Wrapper1LoadViewModel wrapper1 in dataGrid.SelectedItems.Cast<Wrapper1LoadViewModel>().Where(e => e.Load.ParentId != null)) {
                        wrapper2.Load.Mark = wrapper1 != wrapper2 && wrapper1.Load.ParentId == wrapper2.Load.ParentId;
                    }
                }
            }
            DispatchCommands.OnCanExecuteChanged();
        }

        #endregion [ Selection ]

        #region [ ToolBar ]

        protected void Refresh_Clicked(object sender, RoutedEventArgs e) {
            _loadsInitCompleted =
                _trailersInitCompleted =
                _warehousesInitCompleted =
                _yardsInitCompleted =
                _trucksInitCompleted =
                _driversInitCompleted =
                _datCompaniesInitCompleted =
                _datDriversInitCompleted =
                _setupPacketsInitCompleted =
                _totalCarrierHistoryInitCompleted =
                _totalBrokerHistoryInitCompleted = false;
            _initialized = false;
            Model.Loading = true;
            //Model.SelectedPatterns = new ObservableCollection<object>();
            //Model.SelectedLoads = new ObservableCollection<object>();
            StaticUninitialize();
            StaticInitialize();
        }

        protected void FiltersPatterns_Clicked(object sender, RoutedEventArgs e) {
            OpenPatternsFiltersAction();
        }

        protected void FiltersRecords_Clicked(object sender, RoutedEventArgs e) {
            OpenRecordsFiltersAction();
        }

        protected void MyCarriers_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<DatCarriersDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new DatCarriersDialog(
                    _dispatchService,
                    _settings,
                    Model.SelectedBrokerCompany,
                    DispatchBoardViewModel.Instance.DatCompanies.ToList());
                dialog.Show();
            }
        }

        protected void MyBrokers_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<DatBrokersDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                var dialog = new DatBrokersDialog(_dispatchService, _settings, Model.SelectedCarrierCompany, DispatchBoardViewModel.Instance.DatCompanies.ToList());
                dialog.Show();
            }
        }

        protected void AllCarriers_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<CensusOfCompaniesDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                // uses DispatchBoardViewModel for updating smalllogo
                var dialog = new CensusOfCompaniesDialog(DatCompanyType.Carrier, _mapper, _settings, Model.SelectedBrokerCompany, DispatchBoardViewModel.Instance.DatCompanies/*.ToList()*/);
                dialog.Show();
            }
        }

        protected void AllBrokers_Clicked(object sender, RoutedEventArgs e) {
            var exists = Application.Current.Windows.OfType<CensusOfCompaniesDialog>().FirstOrDefault();
            if (exists != null) {
                exists.Focus();
            } else {
                // uses DispatchBoardViewModel for updating smalllogo 
                var dialog = new CensusOfCompaniesDialog(DatCompanyType.Broker, _mapper, _settings, Model.SelectedCarrierCompany, DispatchBoardViewModel.Instance.DatCompanies/*.ToList()*/);
                dialog.Show();
            }
        }

        #endregion [ ToolBar ]

        #region [ Filter ]

        // *** FILTERING LOADS ***
        // Monitoring Duration: 2.6sec / 1calls
        // ViewSource_OnFilter: 24%, 0.6s
        // Static_ViewSource_OnFilter: 24%, 0.6s
        // без мониторинг ~2sec. оптимизация не требуется.
        // Contains и IndexOf работают одинаково
        protected bool ViewSource_OnFilter(string search, FilterColumnsViewModel filterColumns, Wrapper1LoadViewModel wrapper) {
            // a Filter by board
            var result1_1 = wrapper != null && wrapper.Load != null && wrapper.Load.BoardType == DispatchBoardType &&
                (wrapper.ActualCompany == null ||
                    (this.DispatchBoardType == DispatchBoardType.Carrier && Model.SelectedCarrierCompany == null || Model.SelectedCarrierCompany == wrapper.ActualCompany) ||
                    (this.DispatchBoardType == DispatchBoardType.Broker && Model.SelectedBrokerCompany == null || Model.SelectedBrokerCompany == wrapper.ActualCompany) ||
                    (this.DispatchBoardType == DispatchBoardType.CarrierAccount && Model.SelectedCarrierCompany == null || Model.SelectedCarrierCompany == wrapper.ActualCompany) ||
                    (this.DispatchBoardType == DispatchBoardType.BrokerAccount && Model.SelectedBrokerCompany == null || Model.SelectedBrokerCompany == wrapper.ActualCompany)
                );
            // Sub-Shipment не должен фильтроваться по ActualCompany. так же изменяется DispatchBoardType
            var result1_2 =
                wrapper != null && wrapper.Load != null &&
                (DispatchBoardType == DispatchBoardType.Carrier || DispatchBoardType == DispatchBoardType.CarrierAccount) &&
                (wrapper.Load.BoardType == DispatchBoardType.Broker || wrapper.Load.BoardType == DispatchBoardType.BrokerAccount) &&
                wrapper.Load.ShipmentId != null;
            var result1_3 =
                wrapper != null && wrapper.Load != null &&
                (DispatchBoardType == DispatchBoardType.Broker || DispatchBoardType == DispatchBoardType.BrokerAccount) &&
                (wrapper.Load.BoardType == DispatchBoardType.Carrier || wrapper.Load.BoardType == DispatchBoardType.CarrierAccount) &&
                wrapper.Load.ShipmentId != null;
            // a Filter by Search Text
            var result2 = (
              wrapper == null ||
              String.IsNullOrEmpty(search) ||
             (wrapper.Filter(search)) ||
             (wrapper.Load.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Broker && wrapper.DatCompany != null && wrapper.DatCompany.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Broker && wrapper.DatDriver != null && wrapper.DatDriver.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Carrier && wrapper.Warehouse != null && wrapper.Warehouse.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Carrier && wrapper.Trailer != null && wrapper.Trailer.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Carrier && wrapper.Truck != null && wrapper.Truck.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.Carrier && wrapper.Driver != null && wrapper.Driver.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.BrokerAccount && wrapper.HistoryDatCompany != null && wrapper.HistoryDatCompany.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.BrokerAccount && wrapper.HistoryDatDriver != null && wrapper.HistoryDatDriver.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.CarrierAccount && wrapper.HistoryWarehouse != null && wrapper.HistoryWarehouse.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.CarrierAccount && wrapper.HistoryTrailer != null && wrapper.HistoryTrailer.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.CarrierAccount && wrapper.HistoryTruck != null && wrapper.HistoryTruck.Filter(search)) ||
             (wrapper.Load.BoardType == DispatchBoardType.CarrierAccount && wrapper.HistoryDriver != null && wrapper.HistoryDriver.Filter(search))
            );
            // a Filter by Columns
            var result3 = (
              wrapper == null || filterColumns == null || filterColumns.Columns.Any() == false || (
             (wrapper.Load.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Broker || wrapper.DatCompany == null || wrapper.DatCompany.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Broker || wrapper.DatDriver == null || wrapper.DatDriver.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Carrier || wrapper.DestinationWarehouse == null || wrapper.DestinationWarehouse.FilterDestinationWarehouse(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Carrier || wrapper.Warehouse == null || wrapper.Warehouse.FilterWarehouse(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Carrier || wrapper.Trailer == null || wrapper.Trailer.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Carrier || wrapper.Truck == null || wrapper.Truck.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.Carrier || wrapper.Driver == null || wrapper.Driver.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.BrokerAccount || wrapper.HistoryDatCompany == null || wrapper.HistoryDatCompany.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.BrokerAccount || wrapper.HistoryDatDriver == null || wrapper.HistoryDatDriver.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.CarrierAccount || wrapper.HistoryWarehouse == null || wrapper.HistoryWarehouse.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.CarrierAccount || wrapper.HistoryTrailer == null || wrapper.HistoryTrailer.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.CarrierAccount || wrapper.HistoryTruck == null || wrapper.HistoryTruck.Filter(filterColumns)) &&
             (wrapper.Load.BoardType != DispatchBoardType.CarrierAccount || wrapper.HistoryDriver == null || wrapper.HistoryDriver.Filter(filterColumns)) &&
             // to skip rows
             !(filterColumns.Columns.Any(e => e.Column == FilterColumnEnum.DestinationWarehouse && wrapper.DestinationWarehouse == null)) &&
             !(filterColumns.Columns.Any(e => e.Column == FilterColumnEnum.Warehouse && wrapper.Warehouse == null)) &&
             !(filterColumns.Columns.Any(e => e.Column == FilterColumnEnum.Trailer && wrapper.Trailer == null)) &&
             !(filterColumns.Columns.Any(e => e.Column == FilterColumnEnum.Truck && wrapper.Truck == null)) &&
             !(filterColumns.Columns.Any(e => e.Column == FilterColumnEnum.Driver && wrapper.Driver == null))
            ));

            return (result1_1 || result1_2 || result1_3) && result2 && result3 &&
                    ((_session.Roles & (int)RoleName.Agent) != (int)RoleName.Agent || wrapper.Load.CreatedById == _session.UserId);
        }

        public static DispatchFilterLoadType Static_ViewSource_OnFilter(Wrapper1LoadViewModel wrapper) {
            //Write($"S{wrapper?.Load?.IndexString} ");
            if (wrapper.Load.ParentId == null) {
                // Shipments SubLoad
                //if (wrapper.Load.ShipmentId != null) {
                //    var shipment = DispatchBoardViewModel.Instance.Wrappers.FirstOrDefault(e => e.Load.EntityId == wrapper.Load.ShipmentId);
                //    if (shipment != null) {
                //        switch (shipment.Load.BoardType) {
                //            case DispatchBoardType.Carrier:
                //                return DispatchFilterLoadType.Sub_Carrier_Patterns;
                //            case DispatchBoardType.Broker:
                //                return DispatchFilterLoadType.Sub_Broker_Patterns;
                //            case DispatchBoardType.CarrierAccount:
                //                return DispatchFilterLoadType.Sub_Accounting_Carriers;
                //            case DispatchBoardType.BrokerAccount:
                //                return DispatchFilterLoadType.Sub_Accounting_Brokers;
                //            default:
                //                throw new Exception();
                //        }
                //    } else {
                //        switch (wrapper.Load.BoardType) {
                //            case DispatchBoardType.Carrier:
                //                return DispatchFilterLoadType.Carrier_Records;
                //            case DispatchBoardType.Broker:
                //                return DispatchFilterLoadType.Broker_Records;
                //            case DispatchBoardType.CarrierAccount:
                //                return DispatchFilterLoadType.Accounting_Carriers;
                //            case DispatchBoardType.BrokerAccount:
                //                return DispatchFilterLoadType.Accounting_Brokers;
                //            default:
                //                throw new Exception();
                //        }
                //    }
                //}
                if (wrapper.Children.Any() == false) {
                    // Simple Load
                    switch (wrapper.Load.BoardType) {
                        case DispatchBoardType.Carrier:
                            if (wrapper.Trailer == null &&
                                wrapper.Truck == null &&
                                wrapper.Driver == null &&
                                wrapper.Warehouse == null &&
                                wrapper.Shipment == null) {
                                return DispatchFilterLoadType.Carrier_Records;
                            } else {
                                return DispatchFilterLoadType.Carrier_Patterns;
                            }
                        case DispatchBoardType.Broker:
                            if (wrapper.DatCompany == null && wrapper.DatDriver == null) {
                                return DispatchFilterLoadType.Broker_Records;
                            } else {
                                return DispatchFilterLoadType.Broker_Patterns;
                            }
                        case DispatchBoardType.CarrierAccount:
                            return DispatchFilterLoadType.Accounting_Carriers;
                        case DispatchBoardType.BrokerAccount:
                            return DispatchFilterLoadType.Accounting_Brokers;
                        default:
                            throw new Exception();
                    }
                } else {
                    // Container
                    // не учитывает assigled object
                    switch (wrapper.Load.BoardType) {
                        case DispatchBoardType.Carrier:
                            // child
                            if (DispatchHelper.GetAssignedId(wrapper) != null) {
                                return DispatchFilterLoadType.Carrier_Patterns;
                            }
                            // root
                            var allAssigned = wrapper.Children.All(e => DispatchHelper.GetAssignedId(e) != null);
                            if (allAssigned) {
                                return DispatchFilterLoadType.Carrier_Patterns;
                            } else {
                                return DispatchFilterLoadType.Carrier_Records;
                            }
                        case DispatchBoardType.Broker:
                            if (DispatchHelper.GetAssignedId(wrapper) != null) {
                                return DispatchFilterLoadType.Broker_Patterns;
                            }
                            var allAssigned2 = wrapper.Children.All(e => DispatchHelper.GetAssignedId(e) != null);
                            if (allAssigned2) {
                                return DispatchFilterLoadType.Broker_Patterns;
                            } else {
                                return DispatchFilterLoadType.Broker_Records;
                            }
                        case DispatchBoardType.CarrierAccount:
                            return DispatchFilterLoadType.Accounting_Carriers;
                        case DispatchBoardType.BrokerAccount:
                            return DispatchFilterLoadType.Accounting_Brokers;
                        default:
                            throw new Exception();
                    }
                }
            } else {
                var parent = DispatchBoardViewModel.Instance.Wrappers.FirstOrDefault(e => e.Load.EntityId == wrapper.Load.ParentId);
                if (parent != null) {
                    /*
                        если Children непривязан, а Root привязан, то Children туда же
                    !!!!! как будет работать на разных бордах?????
                        */
                    switch (wrapper.Load.BoardType) {
                        case DispatchBoardType.Carrier:
                            if (wrapper.Trailer == null &&
                                wrapper.Truck == null &&
                                wrapper.Driver == null &&
                                wrapper.Warehouse == null &&
                                wrapper.Shipment == null) {
                                return Static_ViewSource_OnFilter(parent) == DispatchFilterLoadType.Carrier_Records ?
                                    DispatchFilterLoadType.Sub_Carrier_Records :
                                    DispatchFilterLoadType.Sub_Carrier_Patterns;
                            } else {
                                return Static_ViewSource_OnFilter(parent) == DispatchFilterLoadType.Carrier_Patterns ?
                                    DispatchFilterLoadType.Sub_Carrier_Patterns :
                                    DispatchFilterLoadType.Carrier_Patterns;
                            }
                        case DispatchBoardType.Broker:
                            if (wrapper.DatCompany == null && wrapper.DatDriver == null) {
                                return Static_ViewSource_OnFilter(parent) == DispatchFilterLoadType.Broker_Records ?
                                    DispatchFilterLoadType.Sub_Broker_Records :
                                    DispatchFilterLoadType.Sub_Broker_Patterns;
                            } else {
                                return Static_ViewSource_OnFilter(parent) == DispatchFilterLoadType.Broker_Patterns ?
                                    DispatchFilterLoadType.Sub_Broker_Patterns :
                                    DispatchFilterLoadType.Broker_Patterns;
                            }
                        case DispatchBoardType.CarrierAccount:
                            return parent.Load.BoardType == wrapper.Load.BoardType ?
                                DispatchFilterLoadType.Sub_Accounting_Carriers :
                                DispatchFilterLoadType.Accounting_Carriers;
                        case DispatchBoardType.BrokerAccount:
                            return parent.Load.BoardType == wrapper.Load.BoardType ?
                                DispatchFilterLoadType.Sub_Accounting_Brokers :
                                DispatchFilterLoadType.Accounting_Brokers;
                        default:
                            throw new Exception();
                    }
                } else {
                    switch (wrapper.Load.BoardType) {
                        case DispatchBoardType.Carrier:
                            if (wrapper.Trailer == null &&
                                wrapper.Truck == null &&
                                wrapper.Driver == null &&
                                wrapper.Warehouse == null &&
                                wrapper.Shipment == null) {
                                return DispatchFilterLoadType.Carrier_Records;
                            } else {
                                return DispatchFilterLoadType.Carrier_Patterns;
                            }
                        case DispatchBoardType.Broker:
                            if (wrapper.DatCompany == null && wrapper.DatDriver == null) {
                                return DispatchFilterLoadType.Broker_Records;
                            } else {
                                return DispatchFilterLoadType.Broker_Patterns;
                            }
                        case DispatchBoardType.CarrierAccount:
                            return DispatchFilterLoadType.Accounting_Carriers;
                        case DispatchBoardType.BrokerAccount:
                            return DispatchFilterLoadType.Accounting_Brokers;
                        default:
                            throw new Exception();
                    }
                }
            }
        }

        #endregion [ Filter ]

        #region [ edit cell ]

        public static RoutedEventHandler Paid_OnClick => _paid_OnClick;

        public static void _paid_OnClick(object sender, RoutedEventArgs args) {
            var button = (Button)sender;
            var wrapper = (Wrapper1LoadViewModel)button.DataContext;
            if (button.Tag is ExpenseType)
                wrapper.Load.ExpenseInvertPaid((ExpenseType)button.Tag);
            if (button.Tag is GainType)
                wrapper.Load.GainInvertPaid((GainType)button.Tag);
        }

        private string OldDataExpense, OldDataGain;
        protected void grid_PreparedCellForEdit(object sender, GridViewPreparingCellForEditEventArgs args) {
            var un = args.Column.UniqueName;
            var w = (Wrapper1LoadViewModel)args.Row.DataContext;
            var load = un.StartsWith("LinkedWrapper") ? w.LinkedWrapper?.Load : w.Load;
            if (load == null) return;
            // subload is readonly
            Match match;
            if ((match = Regex.Match(un, @"Load.ExpensesView\[(?<index>[\d+])\].Cost")).Success) {
                if (load.Parent1 != null) {
                    args.Row.GridViewDataControl.CancelEdit();
                    return;
                }
                var expense = load.Expenses[Int32.Parse(match.Groups["index"].Value)];
                OldDataExpense = expense.Cost?.ToString() + expense.Paid.ToString();
            }
            if ((match = Regex.Match(un, @"Load.GainsView\[(?<index>[\d+])\].Cost")).Success) {
                if (load.Parent1 != null) {
                    args.Row.GridViewDataControl.CancelEdit();
                    return;
                }
                var gain = load.Gains[Int32.Parse(match.Groups["index"].Value)];
                OldDataGain = gain.Cost?.ToString() + gain.Paid.ToString();
            }
            if ((match = Regex.Match(un, @"Load.ExpensesView\[(?<index>[\d+])\].Comment")).Success) {
                if (load.Parent1 != null) {
                    args.Row.GridViewDataControl.CancelEdit();
                    return;
                }
            }
            if ((match = Regex.Match(un, @"Load.GainsView\[(?<index>[\d+])\].Comment")).Success) {
                if (load.Parent1 != null) {
                    args.Row.GridViewDataControl.CancelEdit();
                    return;
                }
            }
            if (un == "SetupPacket.MailingCompanyLegalName") {
                if (((Wrapper1LoadViewModel)args.Row.DataContext).SetupPacket == null) {
                    args.Row.GridViewDataControl.CancelEdit();
                    return;
                }
            }
        }

        // private Task task;
        protected virtual void grid_CellEditEnded(object sender, GridViewCellEditEndedEventArgs args) {
            if (args.EditAction == GridViewEditAction.Cancel) {
                return;
            }
            var wrapper = (Wrapper1LoadViewModel)args.Cell.DataContext;
            var un = args.Cell.Column.UniqueName;
            if (un.StartsWith("LinkedWrapper")) {
                wrapper = wrapper.LinkedWrapper;
            }
            if (wrapper == null) {
                return;
            }

            // Expenses/Gains ------------------------------->
            // ручная проверка на одинаковый Old и New
            Match match;
            if ((match = Regex.Match(un, @"Load.ExpensesView\[(?<index>[\d+])\].Cost")).Success) {
                var expense = wrapper.Load.Expenses[Int32.Parse(match.Groups["index"].Value)];
                var newDataExpense = expense.Cost?.ToString() + expense.Paid.ToString();
                if (OldDataExpense != newDataExpense) {
                    this.CellEditEnded(wrapper, args);
                }
                return;
            }
            if ((match = Regex.Match(un, @"Load.GainsView\[(?<index>[\d+])\].Cost")).Success) {
                var expense = wrapper.Load.Gains[Int32.Parse(match.Groups["index"].Value)];
                var newDataGain = expense.Cost?.ToString() + expense.Paid.ToString();
                if (OldDataGain != newDataGain) {
                    this.CellEditEnded(wrapper, args);
                }
                return;
            }
            // <-----------------------------------------

            // NOTES: Sometimes Old and New values of Status are equal but really its are different
            // DON'T USE THIS CODE
            //if (Object.Equals(args.OldData, args.NewData) || ((args.OldData == null ||
            //    args.OldData.Equals("")) && (args.NewData == null || args.NewData.Equals("")))) {
            //    return;
            //}

            this.CellEditEnded(wrapper, args);
        }

        protected virtual void CellEditEnded(Wrapper1LoadViewModel wrapper, GridViewCellEditEndedEventArgs args) {
            var un = args.Cell.Column.UniqueName;
            // Inputted Expense or Gain
            Match match;

            if ((match = Regex.Match(un, @"Load.ExpensesView\[(?<index>[\d+])\].Cost")).Success) {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if ((match = Regex.Match(un, @"Load.ExpensesView\[(?<index>[\d+])\].Comment")).Success) {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if ((match = Regex.Match(un, @"Load.GainsView\[(?<index>[\d+])\].Cost")).Success) {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if ((match = Regex.Match(un, @"Load.GainsView\[(?<index>[\d+])\].Comment")).Success) {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if (un == @"Load.NewScheduledAppointment") {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if (un == @"Load.Spots") {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
            if (un == @"Load.Commodity") {
                _dispatchService.SaveLoad(wrapper.Load, SaveCompleted);
            }
        }

        protected void grid_CellValidating(object sender, GridViewCellValidatingEventArgs args) {
            if (args.Cell.Column.UniqueName == "Load.Index") {
                if (String.IsNullOrEmpty((string)args.NewValue)) {
                    args.IsValid = false;
                    args.ErrorMessage = "Load# can not be null";
                }
            }
            // https://docs.telerik.com/devtools/wpf/controls/radgridview/columns/celltemplate-and-celledittemplate
            // NewValue не будет задан если не переопределить колонку
            Match match;
            if ((match = Regex.Match(args.Cell.Column.UniqueName, @"Load.ExpensesView\[(?<index>[\d+])\].Cost")).Success) {
                args.IsValid = args.NewValue == null || Object.Equals(args.NewValue, "") || args.NewValue is decimal;
            }
            if ((match = Regex.Match(args.Cell.Column.UniqueName, @"Load.GainsView\[(?<index>[\d+])\].Cost")).Success) {
                args.IsValid = args.NewValue == null || Object.Equals(args.NewValue, "") || args.NewValue is decimal;
            }
        }

        #endregion [ edit cell ]

        #region [ IDispatchControl ]

        public async Task SetLoads(List<LoadViewModel> loads) {
            await Model.SetLoadsAsync(loads);
        }

        #endregion [ IDispatchControl ]

        #region [ Actions ]

        protected void OpenPatternsFiltersAction() {
            DispatchFilterTableType filterType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.Carrier:
                    filterType = DispatchFilterTableType.Carrier_Patterns;
                    break;

                case DispatchBoardType.Broker:
                    filterType = DispatchFilterTableType.Broker_Patterns;
                    break;

                default:
                    throw new Exception("undefined type");
            }

            FiltersDialog dlg = new FiltersDialog(_settings, _mapper, (FiltersColumnsViewModel)Model.FiltersColumns[filterType].Clone());
            if (dlg.ShowDialog() == true) {
                Model.SetFiltersColumns(filterType, dlg.Model);
            }
        }

        protected void OpenRecordsFiltersAction() {
            DispatchFilterTableType filterType;
            switch (this.DispatchBoardType) {
                case DispatchBoardType.Carrier:
                    filterType = DispatchFilterTableType.Carrier_Records;
                    break;

                case DispatchBoardType.Broker:
                    filterType = DispatchFilterTableType.Broker_Records;
                    break;

                case DispatchBoardType.CarrierAccount:
                    filterType = DispatchFilterTableType.Accounting_Carriers;
                    break;

                case DispatchBoardType.BrokerAccount:
                    filterType = DispatchFilterTableType.Accounting_Brokers;
                    break;

                case DispatchBoardType.CarrierHistory:
                    filterType = DispatchFilterTableType.Carrier_History;
                    break;

                default:
                    throw new Exception("undefined type");
            }

            FiltersDialog dlg = new FiltersDialog(_settings, _mapper, (FiltersColumnsViewModel)Model.FiltersColumns[filterType].Clone());
            if (dlg.ShowDialog() == true) {
                Model.SetFiltersColumns(filterType, dlg.Model);
            }
        }

        protected async void ExportAction(RadGridView grid, string fileName) {

            var showSubloads = MessageBox.Show("Do you want to show SubLoads?", "Export to Excel", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // NOTES: serializes ItemsSource
            var itemsSource = grid.Items.Cast<Wrapper1LoadViewModel>().ToList();
            if (showSubloads == MessageBoxResult.Yes) {
                foreach (Wrapper1LoadViewModel item in itemsSource.ToList()) {
                    if (item.Children.Any()) {
                        var index = itemsSource.IndexOf(item);
                        itemsSource.Remove(item);
                        foreach (var child in item.Children.Reverse()) {
                            itemsSource.Insert(index, child);
                        }
                    }
                }
            }

            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "xlsx";
            dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            dialog.FileName = fileName;

            if (dialog.ShowDialog() == true) {
                var columns = grid.Columns
                        .Cast<GridViewBoundColumnBase>()
                        .Where(e => e.IsVisible)
                        .OrderBy(e => e.DisplayIndex)
                        .Select(e => new ExportExcelColumn() {
                            UniqueName = e.UniqueName,
                            BindingPath = e.DataMemberBinding.Path.Path,
                            StringFormat = e.DataMemberBinding.StringFormat,
                            Width = e.Width.DisplayValue,
                            Group = e.ColumnGroupName,
                            Header = e.Header.ToString()
                        })
                        .ToList();

                try {
                    BusyStart("Saving...");
                    await Task.Delay(TimeSpan.FromSeconds(0.2));

                    ExportHelper.CreateFile(
                        dialog.FileName,
                        DispatchBoardType,
                        itemsSource,
                        columns);
                    Process.Start(new ProcessStartInfo(Path.GetDirectoryName(dialog.FileName)) { UseShellExecute = true });
                } finally {
                    BusyFinish();
                }
            }
        }

        #endregion [ Actions ]

        #region [ Hyperlink File ]

        // TODO: move to DispatchActions
        protected void OpenHyperlinkFile_OnClick(object sender, RoutedEventArgs e) {
            var company = ((Wrapper1LoadViewModel)((FrameworkContentElement)sender).DataContext).DatCompany;
            if (company != null) {
                this.DispatchActions.FileLoad(new FileViewModel {
                    EntityId = company.EntityId,
                    Name = company.HyperlinkFile
                });
            }
        }

        #endregion [ Hyperlink File ]
    }
}
