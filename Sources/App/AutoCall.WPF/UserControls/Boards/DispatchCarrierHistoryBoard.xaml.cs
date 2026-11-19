using AutoCall.Common;
using AutoCall.Common.Collections;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Data;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Contracts.Settings.Sections;
using AutoCall.Logic.Managers;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Settings.Sections;
using AutoCall.WPF.Dialogs.Dispatch;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.Helpers;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Dtos.Responses;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.DataServices.Responses;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Heplers;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using Application = System.Windows.Application;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using GridViewColumnCollection = Telerik.Windows.Controls.GridViewColumnCollection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ResponseStatus = AutoDispatch.Dtos.Enums.ResponseStatus;
using AutoDispatch.Logic.Converters;

namespace AutoCall.WPF.UserControls.Boards {

    public partial class DispatchCarrierHistoryBoard : DispatchBoardBase {

        #region [ _fields ]

        public override DispatchBoardType DispatchBoardType => DispatchBoardType.CarrierHistory;
        public override Slider ScaleSlider => this.uiScaleSlider;
        public override ScaleTransform ScaleTransform => this.scaleTransform;

        private readonly CompositeFilterDescriptor compositeFilter;
        private HelperDataService _helperService => App.Services.GetRequiredService<HelperDataService>();
        private DatCompanyDataService _datCompanyService => App.Services.GetRequiredService<DatCompanyDataService>();

        #endregion [ _fields ]

        #region [ Properties ]

        protected override LoadingControl loading1 => this.loading;

        #endregion [ Properties ]

        #region [ Model ]

        public CarrierHistoryViewModel HistoryModel { get; set; } = new CarrierHistoryViewModel();

        #endregion [ Model ]

        #region [ .ctor ]

        public DispatchCarrierHistoryBoard(ModuleName moduleName, BoardType boardType, MainManager manager, SettingsService settings)
            : base(moduleName, boardType, manager, settings) {

            InitializeComponent();
            acOrigin.SelectionChanged += acLocation_SelectionChanged;
            acDestination.SelectionChanged += acLocation_SelectionChanged;
            this.gridLoads.KeyboardCommandProvider = new CustomKeyboardCommandProvider(this.gridLoads);

            DataContext = Model;

            // The Filter for history
            compositeFilter = new CompositeFilterDescriptor() { LogicalOperator = FilterCompositionLogicalOperator.Or };
            compositeFilter.FilterDescriptors.Add(new FilterDescriptor("Load.IsDeleted", FilterOperator.IsEqualTo, true));
        }

        #endregion [ .ctor ]

        #region [ Initialize ]

        private void DispatchBoardBase_Loaded(object sender, RoutedEventArgs e) {
            // Assigns Items Source
            gridLoads.ItemsSource = ((CollectionViewSource)Resources["LoadsViewSource"]).View;
            HistoryModel.PageChanged += HistoryModel_PageChanged;
        }

        protected virtual void AddColumns() {
            {
                var col = gridLoads.CreateImageColumn("", "ActualCompany.LogoSmall");
                PropertyExtensions.SetRelativeDisplayIndex(col, 1);
                col.ColumnGroupName = "Load";
                col.IsResizable = false;
                col.Width = 30;
            }
            {
                var col = gridLoads.CreateTextColumn("Company Name", "ActualCompany.LegalName");
                PropertyExtensions.SetRelativeDisplayIndex(col, 2);
                col.ColumnGroupName = "Load";
                col.Width = 100;
            }
        }

        public override void InitializeGrids() {
            this.AddColumns();

            //BusyStart("Loading..."); // binding with Model.Loading will be broken
            loading1.LoadingMessage = "Loading...";

            // *** Applies settings of columns AFTER after was their added ***
            base.InitializeGrids();
        }

        private void RadContextMenu_Opening(object sender, RadRoutedEventArgs e) {
            if ((RoleName)_session.Roles != RoleName.Admin) {
                if (((Wrapper1LoadViewModel)((FrameworkElement)sender).DataContext).Load.Parent1 != null) {
                    e.Handled = true;
                }
            }
        }

        #endregion [ Initialize ]

        #region [ Engine Events ]

        protected override async Task _engine_EventReceived(EventResponse response) {
            await base._engine_EventReceived(response);
            if (this.Dispatcher.CheckAccess() == true) {
                gridLoads.CalculateAggregates();
            }
        }

        #endregion [ Engine Events ]

        #region [ Settings ]

        public override void SetSettings(BoardSettings2 settings) {
            var tableName = SettingsTableNames.History_Carriers;
            GridViewColumnsHelper.LoadAndBindSettingsViewModel(_mapper, tableName, settings.RecordColumnSettings2, gridLoads, Model.GridsSettings);
            GridViewColumnsHelper.ReorderGroupRelative(gridLoads);
        }

        public override BoardSettings2 GetSettings() {
            var tableName = SettingsTableNames.History_Carriers;
            GridSettingsViewModel data2 = Model.GridsSettings.Grids[tableName];
            return new BoardSettings2() {
                RecordColumnSettings2 = _mapper.Map<GridViewSettings>(data2),
            };
        }

        #endregion [ Settings ]

        #region [ Factoring Company ]

        /// <summary>
        ///     Cache </summary>
        private List<SetupPacketFactoringCompanyDto> FactoringCompanies = null;

        private async void cbFactoringCompany_Loaded(object sender, RoutedEventArgs args) {
            var comboBox = (RadComboBox)sender;
            StyleManager.SetTheme(comboBox, new VistaTheme());
            comboBox.IsEnabled = false;
            if (FactoringCompanies == null) {
                GetFactoringCompaniesResponse response = await App.Services.GetRequiredService<DatCompanyDataService>().GetFactoringCompanies(new GetFactoringCompaniesRequest());
                if (response.IsSuccessed) {
                    FactoringCompanies = response.Data;
                } else {
                    ((GridViewEditorPresenter)comboBox.Parent).IsContentValid = false;
                    ((GridViewEditorPresenter)comboBox.Parent).ErrorMessage = response.ErrorMessage;
                    return;
                }
            }
            comboBox.IsEnabled = FactoringCompanies != null;
            comboBox.ItemsSource = FactoringCompanies;
        }

        #endregion [ Factoring Company ]

        #region [ SubLoads Loaded ]

        private void ChildGridLoads_OnLoaded(object sender, RoutedEventArgs args) {
            var gridView = (RadGridView)sender;
            // Subscription doesn't worked in a markup
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter -= SubLoadsViewSource_OnFilter;
            ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).Filter += SubLoadsViewSource_OnFilter;
            // Sets CollectionView
            gridView.ItemsSource = ((CollectionViewSource)gridView.Resources["SubLoadsViewSource"]).View;
            ((ICollectionView)gridView.ItemsSource).Refresh();
            // NOTES: Selects last row on last subload
            this.SelectedItemsModel.SelectedLoads.Clear();
            this.SelectedItemsModel.SelectedPatterns.Clear();

            GridViewColumnCollection parentColumns = gridLoads.Columns;
            GridViewColumnCollection childColumns = gridView.Columns;

            // doesn't need to add columns if its exists
            if (gridView.Columns.Count > 0) {
                return;
            }

            gridView.RowStyle = new Style(typeof(GridViewRow), gridLoads.RowStyle);
            gridView.RowStyle.Setters.Add(new Setter(GridViewRow.IsExpandableProperty, false));
            gridView.HierarchyChildTemplate = gridLoads.HierarchyChildTemplate;

            // ViewModel of GridViewColumns
            GridSettingsViewModel parentGridViewSettingsViewModel =
                Model.GridsSettings.Grids[SettingsTableNames.History_Carriers];

            foreach (GridViewColumn parentColumn in parentColumns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex)) {
                GridViewColumnsHelper.SyncChildColumn(parentColumn, parentGridViewSettingsViewModel, childColumns);
            }
        }

        #endregion [ SubLoads Loaded ]

        #region [ DoubleClick Events ]

        private bool _eventHandled = false;
        private void gridLoads_CellDoubleClick(object sender, RadRoutedEventArgs e) {
        }

        private void gridLoads_RowDoubleClick(object sender, MouseButtonEventArgs e) {
            // NOTE: the event triggers twice. Checks by one
            if ((e.OriginalSource as FrameworkContentElement)?.DataContext != ((FrameworkElement)e.Source).DataContext &&
                (e.OriginalSource as FrameworkElement)?.DataContext != ((FrameworkElement)e.Source).DataContext) {
                return;
            }
            if (_eventHandled == false) {
                var wrapper = (Wrapper1LoadViewModel)((FrameworkElement)e.Source).DataContext;
                this.DispatchActions.EditLoadAction(wrapper, true, this.HistoryModel.ViewModel);
            }
            _eventHandled = false;
        }

        #endregion [ DoubleClick Events ]

        #region [ Filter ]

        // Loads
        protected virtual void LoadsViewSource_OnFilter(object sender, FilterEventArgs args) {
            var wrapper = (Wrapper1LoadViewModel)args.Item;
            var parent = HistoryModel.ViewModel.Wrappers.ToList().FirstOrDefault(e => e.Load.EntityId == wrapper.Load.ParentId);
            args.Accepted = parent == null;
        }

        // SubLoads
        public static FilterEventHandler Static_SubLoadsViewSource_OnFilter;
        protected virtual void SubLoadsViewSource_OnFilter(object sender, FilterEventArgs args) {
            args.Accepted = true;
        }

        protected override void Patterns_FilterColumns_SelectionChanged() { }
        protected override void Records_FilterColumns_SelectionChanged() { }

        #endregion [ Filter ]

        #region [ Reordering columns ]

        private void gridLoads_ColumnReordering(object sender, ColumnReorderingEventArgs args) {
            GridViewColumnsHelper.ReorderGroup((RadGridView)sender, args);
            GridViewColumnsHelper.ReorderGroupRelative((RadGridView)sender);
            args.Cancel = true;
        }

        #endregion [ Reordering columns ]

        #region [ Load Events ]

        private void HistoryModel_PageChanged(object sender, EventArgs e) {
            if (HistoryModel.ItemCount > 0) {
                LoadHistory();
            }
        }

        private void PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (HistoryModel.ItemCount > 0) {
                LoadHistory();
            }
        }

        private void btnSearch_Clicked(object sender, RoutedEventArgs e) {
            LoadHistory();
        }

        private void LoadHistory() {
            var requestDto = _mapper.Map<RequestHistory2Dto>(this.HistoryModel);
            requestDto.ActualCompanyId = Model.SelectedCarrierCompany?.EntityId;
            _dispatchService.LoadHistory2(requestDto, LoadHistory_Completed);
            Model.Loading = true;
        }

        private async Task LoadHistory_Completed(ViewModelWrapper<ResponseHistory2Dto> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                await Dispatcher.BeginInvoke ((Action)(async () => await LoadHistory_Completed(wrapper)));
                return;
            }
            Model.Loading = false;
            if (wrapper.Status == ResponseStatus.Success) {
                this.HistoryModel.ViewModel.Clear();
                this.HistoryModel.ViewModel.SetActualCompanies(wrapper.Data.ActualCompanies?.Select(ActualCompanyConverter.ToVm).ToList() ?? new List<ActualCompanyViewModel>());
                await this.HistoryModel.ViewModel.SetLoadsAsync(wrapper.Data.Loads?.Select(LoadConverter.ToVm).ToList() ?? new List<LoadViewModel>(), true, true);
                this.HistoryModel.PageIndex = wrapper.Data.PageIndex;
                this.HistoryModel.ItemCount = wrapper.Data.Total;
                ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
        }

        private void btnClear_Clicked(object sender, RoutedEventArgs e) {
            this.HistoryModel.ViewModel.Clear();
            ((ICollectionView)this.gridLoads.ItemsSource).Refresh();
        }

        private void Equipment_OnClicked(object sender, RoutedEventArgs e) {
            switch (this.HistoryModel.Equipment) {
                case AutoCallEquipmentType.Van:
                    this.HistoryModel.Equipment = AutoCallEquipmentType.Reefer;
                    break;
                case AutoCallEquipmentType.Reefer:
                    this.HistoryModel.Equipment = null;
                    break;
                default:
                    this.HistoryModel.Equipment = AutoCallEquipmentType.Van;
                    break;
            }
        }

        private void LoadType_OnClicked(object sender, RoutedEventArgs e) {
            switch (this.HistoryModel.LoadType) {
                case LoadType.Full:
                    this.HistoryModel.LoadType = LoadType.Partial;
                    break;
                case LoadType.Partial:
                    this.HistoryModel.LoadType = null;
                    break;
                default:
                    this.HistoryModel.LoadType = LoadType.Full;
                    break;
            }
        }

        #endregion [ Load Events ]

        #region [ AutoComplete ]

        /// <summary>
        ///     placeId подсказок: координаты места подгружаются при выборе (acLocation_SelectionChanged) </summary>
        private readonly Dictionary<LocationViewModel, string> _placeIds = new Dictionary<LocationViewModel, string>();
        private HelperDataService _dataService = App.Services.GetRequiredService<HelperDataService>();

        private Task task = null;
        private bool _processing;
        private FrameworkElement _focused = null;

        /// <summary>
        ///     Search Text changed handler </summary>
        protected async void acLocation_SearchTextChanged(object sender, EventArgs e) {
            #region [ single request + delay ]
            var localTask = task = Task.Delay(1000);
            await task;
            if (localTask != task) {
                return;
            }
            #endregion [ single request + delay ]

            try {
                var ui = (RadAutoCompleteBox)sender;
                if (ui.SelectedItem == null && ui.SearchText?.Length >= 2) {
                    _processing = true;

                    // Get data from service. V1 — города DAT (IDatPowerStorage.GetLocationsAsync, модуль AutoCall) сразу с координатами;
                    // теперь подсказки мест сервера (Api/Dispatch/Helpers/PlaceAutocomplete), координаты — по выбору, как AutoCall V3 (PlaceApi)
                    JArray predictions = await _dataService.GetPlaceAutocomplete(ui.SearchText);

                    // Populate autocomplete items source
                    _placeIds.Clear();
                    ui.ItemsSource = predictions.Cast<JObject>().Select(p => {
                        var location = new LocationViewModel() { FormattedAddress = StringExtension.NormalizeCityName((string)p["description"]) };
                        _placeIds[location] = PlaceAutocompleteJsonParser.GetPlaceId(p);
                        return location;
                    }).ToList();
                    ui.Populate(ui.SearchText);
                }
            } finally {
                _processing = false;
            }
        }

        /// <summary>
        ///     Координаты выбранной подсказки (Api/Dispatch/Helpers/PlaceDetails) </summary>
        private async void acLocation_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (((RadAutoCompleteBox)sender).SelectedItem is LocationViewModel location &&
                _placeIds.TryGetValue(location, out var placeId) && String.IsNullOrEmpty(placeId) == false) {
                var details = await _dataService.GetPlaceDetails(placeId);
                QueryLoadDto place = PlaceAutocompleteJsonParser.GetCityAndAddress(details);
                if (place?.Location != null) {
                    location.Latitude = place.Location.Latitude;
                    location.Longitude = place.Location.Longitude;
                }
            }
        }

        /// <summary>
        ///     Got Focus </summary>
        /// <remarks>FocusManager doesn't work with AutoCompleter. If focused doesn't work too</remarks>
        protected void RadAutoCompleteBox_GotFocus(object sender, RoutedEventArgs e) {
            _focused = (RadAutoCompleteBox)sender;
        }

        /// <summary>
        ///     Lost Focus </summary>
        /// <remarks>FocusManager doesn't work with AutoCompleter. If focused doesn't work too</remarks>
        protected void RadAutoCompleteBox_LostFocus(object sender, RoutedEventArgs e) {
            _focused = null;
        }

        // Showes drop down of brokers
        protected async void brokerRadAutoCompleteBox_SearchTextChanged(object sender, EventArgs args) {
            var ui = (RadAutoCompleteBox)sender;
            if (_focused == null || _focused.Equals(ui) == false) {
                return;
            }

            #region [ single request + delay ]
            var localTask = task = Task.Delay(1000);
            await task;
            if (localTask != task) {
                return;
            }
            #endregion [ single request + delay ]

            try {
                if (ui.SelectedItem == null && ui.SearchText?.Length >= 2) {
                    _processing = true;
                    if (String.IsNullOrEmpty(ui.SearchText)) {
                        return;
                    }

                    this.HistoryModel.BrokerLoading = true;

                    // Get data from service
                    var result = await _datCompanyService.SearchCensusCompaniesAsync(new SearchCensusCompaniesRequest() {
                        Search = _mapper.Map<CensusCompanySearchDto>(
                            new CensusCompanySearchViewModel() {
                                Limit = 5,
                                NAME = ui.SearchText,
                                CompanyType = DatCompanyType.Broker
                            }
                        )
                    });

                    if (result.IsSuccessed) {
                        // Populate autocomplete items source
                        var censusList = result.CensusCompanies.Select(e => CensusConverter.ToDatCompany(e)).ToList();
                        var brokerList = AutoCall.WPF.Helpers.DispatchHelper.SearchDatCompanies(
                            DispatchBoardViewModel.Instance.DatCompanies,
                            null, ui.SearchText, DatCompanyType.Broker);
                        ui.ItemsSource = AutoCall.WPF.Helpers.DispatchHelper.MergeLists(censusList, brokerList);
                        ui.Populate(ui.SearchText);
                    } else {
                        _l.e("Filter_TextChanged", result.ErrorMessage);
                    }
                }
            } finally {
                this.HistoryModel.BrokerLoading = false;
            }
        }

        // Selected broker from census
        protected void brokerRadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs args) {
            var ui = (RadAutoCompleteBox)sender;

            var broker = ui.SelectedItem as DatCompanyViewModel;
            if (broker != null) {
                this.HistoryModel.BrokerName = broker.Name;
            }
        }

        #endregion [ AutoComplete ]
    }
}
