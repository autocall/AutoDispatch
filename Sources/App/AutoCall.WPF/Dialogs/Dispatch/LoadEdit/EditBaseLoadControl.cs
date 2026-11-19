using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common;
using AutoCall.Logic.Services;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoDispatch.Logic.Heplers;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Telerik.Windows.Controls;
using AutoDispatch.Logic.Helpers;
using AutoCall.Logic.Settings;
using AutoCall.Common.Extensions;
using AutoCall.WPF.Extensions;
using AutoCall.Common.Collections;
using AutoCall.Web.Common.Helpers;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using AutoCall.Common.Enums;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.Converters;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public class EditBaseLoadControl : UserControl {

        protected DatCompanyDataService _datCompanyService;
        protected DispatchService _dispatchService;
        protected HelperDataService _helperService;
        protected ICallService _callService;
        protected SettingsService _settings;
        protected IMapper _mapper;
        protected TextBlock _errorTextBlock;

        public EditBookWrapperViewModel Model {
            get { return DataContext as EditBookWrapperViewModel; }
            set { DataContext = value; }
        }

        public void Init(DispatchService dispatchService, ICallService callService, TextBlock errorTextBlock) {
            _dispatchService = dispatchService;
            _callService = callService;
            _errorTextBlock = errorTextBlock;

            _mapper = App.Services.GetRequiredService<IMapper>();
            _settings = App.Services.GetRequiredService<SettingsService>();
            _helperService = App.Services.GetRequiredService<HelperDataService>();
            _datCompanyService = App.Services.GetRequiredService<DatCompanyDataService>();
        }

        protected void btnOriginLocation_OnClick(object sender, RoutedEventArgs e) {
            try {
                var loadViewModel = (LoadViewModel)((FrameworkElement)sender).DataContext;
                var dlg = new EditLocationDialog();

                LocationViewModel location;
                if (loadViewModel.OriginLocation != null) {
                    location = (LocationViewModel)loadViewModel.OriginLocation.Clone();
                } else {
                    location = new LocationViewModel();
                }

                dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                    Location = location,
                    Address = loadViewModel.OriginAddress,
                    City = loadViewModel.OriginCity
                });
                dlg.Initialize();
                if (dlg.ShowDialog() == true) {
                    using (SyncOrigin()) {
                        loadViewModel.OriginCity = dlg.Model.Location.City;
                        loadViewModel.OriginAddress = dlg.Model.Location.Address;
                        loadViewModel.OriginLocation = dlg.Model.Location.Location;
                    }
                    CalcDistance();
                }
            } catch (Exception) {
            }
        }

        protected void btnDestinationLocation_OnClick(object sender, RoutedEventArgs e) {
            try {
                var loadViewModel = (LoadViewModel)((FrameworkElement)sender).DataContext;
                var dlg = new EditLocationDialog();

                LocationViewModel location;
                if (loadViewModel.DestinationLocation != null) {
                    location = (LocationViewModel)loadViewModel.DestinationLocation.Clone();
                } else {
                    location = new LocationViewModel();
                }

                dlg.Model = new EditLocationViewModel(new LoadLocationViewModel() {
                    Location = location,
                    Address = loadViewModel.DestinationAddress,
                    City = loadViewModel.DestinationCity
                });
                dlg.Initialize();
                if (dlg.ShowDialog() == true) {
                    using (SyncDestination()) {
                        loadViewModel.DestinationCity = dlg.Model.Location.City;
                        loadViewModel.DestinationAddress = dlg.Model.Location.Address;
                        loadViewModel.DestinationLocation = dlg.Model.Location.Location;
                    }
                    CalcDistance();
                }
            } catch (Exception) {
            }
        }

        protected async void Phone_OnMouseUp(object sender, MouseButtonEventArgs e) {
            var phone = StringExtension.OnlyNumbers((string)((FrameworkElement)sender).DataContext);
            await _callService?.StartCallAsync(_callService.ToPhoneNumber(phone));
        }

        protected void RadColorSelector_SelectedColorChanged(object sender, EventArgs e) {
            var button = ((FrameworkElement)sender).GetVisualParent<RadDropDownButton>().IsOpen = false;
        }

        protected void McNumber_OnClick(object sender, RoutedEventArgs routedEventArgs) {
            var dialog = new CensusOfCompaniesDialog(DatCompanyType.Broker, _mapper, _settings, Model.Load.ActualCompany, DispatchBoardViewModel.Instance.DatCompanies/*.ToList()*/);
            dialog.Model.Search.ICC = Model.Load.McNumber;
            dialog.ShowDialog();

        }

        protected void Currency_OnClicked(object sender, RoutedEventArgs e) {
            Model.Load.Currency = (CurrencyType)(((byte)Model.Load.Currency + 1) % Enum.GetValues(typeof(CurrencyType)).Length);
        }

        protected void WeightType_OnClicked(object sender, RoutedEventArgs e) {
            switch (Model.Load.WeightType) {
                case WeightType.kLbs:
                    Model.Load.WeightType = WeightType.Lbs;
                    break;
                case WeightType.Lbs:
                default:
                    Model.Load.WeightType = WeightType.kLbs;
                    break;
            }
        }

        #region [ AutoCompleter ]

        private FrameworkElement _focused = null;
        private bool withoutAddress = false;

        /// <summary>
        ///     Got Focus </summary>
        /// <remarks>FocusManager doesn't work with AutoCompleter. If focused doesn't work too</remarks>
        protected void RadAutoCompleteBox_GotFocus(object sender, RoutedEventArgs e) {
            _focused = (RadAutoCompleteBox)sender;
        }

        /// <summary>
        ///     Lost Focus </summary>
        /// <remarks>FocusManager doesn't work with AutoCompleter. If focused doesn't work too</remarks>
        protected async void RadAutoCompleteBox_LostFocus(object sender, RoutedEventArgs e) {
            _focused = null;
            await RadAutoCompleteBox_LostFocusAsync(sender, e);
        }

        /// <summary>
        ///     Lost Focus Task </summary>
        private async Task RadAutoCompleteBox_LostFocusAsync(object sender, RoutedEventArgs e) {
            var autoCompleteBox = (RadAutoCompleteBox)sender;
            if (autoCompleteBox.Tag is CacheLoadIndexType == false) {
                return;
            }
            if ((CacheLoadIndexType)autoCompleteBox.Tag == CacheLoadIndexType.Origin) {
                var originCity = Model.Load.OriginCity;
                if (Model.Load.OriginLocation == null && 
                    String.IsNullOrEmpty(Model.Load.OriginCity) == false && 
                    LocationHelper.CheckLocation(ref originCity)) {
                    using (SyncOrigin()) {
                        Model.Load.OriginCity = originCity;
                    }
                    // use city
                    var searchString = Model.Load.OriginCity;
                    if (String.IsNullOrEmpty(Model.Load.OriginAddress) == false && withoutAddress == false) {
                        // use address and city
                        searchString = $"{Model.Load.OriginAddress}, {Model.Load.OriginCity}";
                    }
                    JArray jArray = await _helperService.GetPlaceAutocomplete(searchString);
                    if (jArray.Count > 0) {
                        // Get data from service
                        var placeId = PlaceAutocompleteJsonParser.GetPlaceId((JObject)jArray.First);
                        if (String.IsNullOrEmpty(placeId) == false) {
                            var result = await _helperService.GetPlaceDetails(placeId);
                            QueryLoadDto location = PlaceAutocompleteJsonParser.GetCityAndAddress(result);
                            using (SyncOrigin()) {
                                Model.Load.OriginLocation = LocationConverter.ToVm(location.Location);
                            }
                            CalcDistance();
                        }
                    } else {
                        if (withoutAddress == false) {
                            // Repeats without Address
                            withoutAddress = true;
                            await RadAutoCompleteBox_LostFocusAsync(sender, e);
                            withoutAddress = false;
                        }
                    }
                }
            }
            if ((CacheLoadIndexType)autoCompleteBox.Tag == CacheLoadIndexType.Destination) {
                var destinationCity = Model.Load.DestinationCity;
                if (Model.Load.DestinationLocation == null &&
                    String.IsNullOrEmpty(Model.Load.DestinationCity) == false && 
                    LocationHelper.CheckLocation(ref destinationCity)) {
                    using (SyncDestination()) {
                        Model.Load.DestinationCity = destinationCity;
                    }
                    // use city
                    var searchString = Model.Load.DestinationCity;
                    if (String.IsNullOrEmpty(Model.Load.DestinationAddress) == false && withoutAddress == false) {
                        // use address and city
                        searchString = $"{Model.Load.DestinationAddress}, {Model.Load.DestinationCity}";
                    }
                    JArray jArray = await _helperService.GetPlaceAutocomplete(searchString);
                    if (jArray.Count > 0) {
                        // Get data from service
                        var placeId = PlaceAutocompleteJsonParser.GetPlaceId((JObject)jArray.First);
                        if (String.IsNullOrEmpty(placeId) == false) {
                            var result = await _helperService.GetPlaceDetails(placeId);
                            var location = PlaceAutocompleteJsonParser.GetCityAndAddress(result);
                            using (SyncDestination()) {
                                Model.Load.DestinationLocation = LocationConverter.ToVm(location.Location);
                            }
                        }
                    } else {
                        if (withoutAddress == false) {
                            // Repeats without Address
                            withoutAddress = true;
                            RadAutoCompleteBox_LostFocus(sender, e);
                            withoutAddress = false;
                        }
                    }
                }
            }
        }

        private Task task = null;
        private bool _processing;

        protected async Task RadAutoCompleteBox_SearchAddressTextChanged(object sender, EventArgs e, Image busyImg) {
            var ui = (RadAutoCompleteBox)sender;
            if (_focused == null || _focused.Equals(ui) == false) {
                return;
            }

            switch ((CacheLoadIndexType)ui.Tag) {
                case CacheLoadIndexType.Origin:
                    using (SyncOrigin()) {
                        Model.Load.OriginLocation = null;
                    }
                    break;
                case CacheLoadIndexType.Destination:
                    using (SyncDestination()) {
                        Model.Load.DestinationLocation = null;
                    }
                    break;
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
                    busyImg.Visibility = Visibility.Visible;
                    // Get data from service
                    JArray result = await _helperService.GetPlaceAutocomplete(ui.SearchText);
                    ui.ItemsSource = result;
                    ui.Populate(ui.SearchText);
                }
            } catch {
            } finally {
                _processing = false;
                busyImg.Visibility = Visibility.Collapsed;
            }
        }

        protected async Task RadAutoCompleteBox_SearchAddressSelectionChanged(object sender, SelectionChangedEventArgs e, Image busyImg) {
            var ui = (RadAutoCompleteBox)sender;
            try {
                if (e.RemovedItems.Count > 0 || e.RemovedItems.Count > 0 && e.AddedItems[0] == e.RemovedItems[0]) {
                    return;
                }

                if (e.AddedItems.Count > 0) {

                    busyImg.Visibility = Visibility.Visible;
                    // Get data from service
                    var placeId = PlaceAutocompleteJsonParser.GetPlaceId((JObject)e.AddedItems[0]);
                    if (String.IsNullOrEmpty(placeId) == false) {
                        //ui.IsEnabled = false;
                        var result = await _helperService.GetPlaceDetails(placeId);
                        var location = PlaceAutocompleteJsonParser.GetCityAndAddress(result);
                        switch ((CacheLoadIndexType)ui.Tag) {
                            case CacheLoadIndexType.Origin:
                                using (SyncOrigin()) {
                                    Model.Load.OriginCity = location.City;
                                    Model.Load.OriginAddress = location.Address;
                                    Model.Load.OriginLocation = LocationConverter.ToVm(location.Location);
                                }
                                break;
                            case CacheLoadIndexType.Destination:
                                using (SyncDestination()) {
                                    Model.Load.DestinationCity = location.City;
                                    Model.Load.DestinationAddress = location.Address;
                                    Model.Load.DestinationLocation = LocationConverter.ToVm(location.Location);
                                }
                                break;
                        }
                        CalcDistance();
                    }
                }
            } finally {
                busyImg.Visibility = Visibility.Collapsed;
            }
        }

        protected async void cacheRadAutoCompleteBox_SearchTextChanged(object sender, EventArgs e) {
            var ui = (RadAutoCompleteBox)sender;
            if (_focused == null || _focused.Equals(ui) == false) {
                return;
            }

            // If user types text to the Company then resets a other fields when he can types new company
            if (ui.DisplayMemberPath == "Company") {
                switch ((CacheLoadIndexType)ui.Tag) {
                    case CacheLoadIndexType.Origin:
                        Model.Load.ShipperId = null;
                        Model.Load.ShipperContacts = new ContactsViewModel();
                        break;
                    case CacheLoadIndexType.Destination:
                        Model.Load.ReceiverId = null;
                        Model.Load.ReceiverContacts = new ContactsViewModel();
                        break;
                }
            }
            if (ui.DisplayMemberPath == "City") {
                switch ((CacheLoadIndexType)ui.Tag) {
                    case CacheLoadIndexType.Origin:
                        Model.Load.OriginLocation = null;
                        break;
                    case CacheLoadIndexType.Destination:
                        Model.Load.DestinationLocation = null;
                        break;
                }
                CalcDistance();
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
                    // Get data from service
                    var result = await _helperService.SearchLoads(
                        new SearchLoadsRequest() {
                            SearchTexts = new Dictionary<string, string>() {
                                { ui.DisplayMemberPath, ui.SearchText }
                            }
                        }
                    );
                    ui.ItemsSource = _mapper.Map<List<QueryLoadViewModel>>(result.Loads);
                    ui.Populate(ui.SearchText);
                }
            } catch {
            } finally {
                _processing = false;
            }
        }

        protected void cacheRadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var ui = (RadAutoCompleteBox)sender;

            var item = ui.SelectedItem as QueryLoadViewModel;
            if (item != null) {
                switch ((CacheLoadIndexType)ui.Tag) {
                    case CacheLoadIndexType.Origin:
                        Model.Load.ShipperCompany = item.Company;
                        Model.Load.OriginCity = item.City;
                        Model.Load.OriginAddress = item.Address;
                        Model.Load.OriginLocation = item.Location;
                        Model.Load.ShipperId = item.CompanyId;
                        Model.Load.ShipperContacts = item.Contacts != null ? ContactsConverter.ToVm(item.Contacts) : new ContactsViewModel();
                        break;
                    case CacheLoadIndexType.Destination:
                        Model.Load.ReceiverCompany = item.Company;
                        Model.Load.DestinationCity = item.City;
                        Model.Load.DestinationAddress = item.Address;
                        Model.Load.DestinationLocation = item.Location;
                        Model.Load.ReceiverId = item.CompanyId;
                        Model.Load.ReceiverContacts = item.Contacts != null ? ContactsConverter.ToVm(item.Contacts) : new ContactsViewModel();
                        break;
                }
                CalcDistance();
            }
        }

        // Showes drop down of brokers
        protected async void brokerRadAutoCompleteBox_SearchTextChanged(object sender, EventArgs args) {
            var ui = (RadAutoCompleteBox)sender;
            if (_focused == null || _focused.Equals(ui) == false) {
                return;
            }

            Model.Load.Contacts = new ContactsViewModel();
            Model.Load.DatBrokerId = null;

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
                    // broker was selected
                    if (Model.Load.DatBrokerId != null) {
                        return;
                    }

                    Model.Load.CompanyLoading = true;

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
                            Model.Load.ActualCompanyId, ui.SearchText, DatCompanyType.Broker);
                        ui.ItemsSource = AutoCall.WPF.Helpers.DispatchHelper.MergeLists(censusList, brokerList);
                        ui.Populate(ui.SearchText);
                    } else {
                        _l.e("Filter_TextChanged", result.ErrorMessage);
                    }
                }
            } catch {
            } finally {
                Model.Load.CompanyLoading = false;
            }
        }

        // Showes drop down of customers
        protected async void customerRadAutoCompleteBox_SearchTextChanged(object sender, EventArgs e) {
            var ui = (RadAutoCompleteBox)sender;
            if (_focused == null || _focused.Equals(ui) == false) {
                return;
            }

            Model.Load.Contacts = new ContactsViewModel();
            Model.Load.CustomerId = null;

            #region [ single request + delay ]
            var localTask = task = Task.Delay(1000);
            await task;
            if (localTask != task) {
                return;
            }
            #endregion [ single request + delay ]
            try {
                // customer was selected
                if (Model.Load.CustomerId != null) {
                    return;
                }

                Model.Load.CompanyLoading = true;

                // Get data from service
                var result = await _helperService.SearchCustomers(new SearchCustomersRequest() {
                    Text = ui.SearchText
                });

                if (result.IsSuccessed) {
                    // Populate autocomplete items source
                    ui.ItemsSource = result.Companies?.Select(CustomerCompanyConverter.ToVm).ToList() ?? new List<CustomerCompanyViewModel>();
                    ui.Populate(ui.SearchText);
                } else {
                    _l.e("Filter_TextChanged", result.ErrorMessage);
                }
            } catch {
            } finally {
                Model.Load.CompanyLoading = false;
            }
        }

        // Gets Brokers by MC and Sets Broker Name when the list has one broker
        protected void tbMcNumber_GotFocus(object sender, RoutedEventArgs e) {
            _focused = (TextBox)sender;
        }

        protected async void tbMcNumber_TextChanged(object sender, TextChangedEventArgs e) {
            var ui = (TextBox)sender;
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
                if (Model.Load.McNumber == null) {
                    return;
                }

                Model.Load.BrokerCompanyMcLoading = true;
                // Get data from service
                var result = await _datCompanyService.SearchCensusCompaniesAsync(new SearchCensusCompaniesRequest() {
                    Search = _mapper.Map<CensusCompanySearchDto>(
                        new CensusCompanySearchViewModel() {
                            Limit = 5,
                            ICC = Model.Load.McNumber,
                            CompanyType = DatCompanyType.Broker
                        }
                    )
                });

                if (result.IsSuccessed) {
                    if (result.CensusCompanies.Count == 1) {
                        Model.Load.Company = result.CensusCompanies.First().NAME;
                    }
                } else {
                    _l.e("tbMcNumber_LostFocus", result.ErrorMessage);
                }
            } catch {
            } finally {
                Model.Load.BrokerCompanyMcLoading = false;
            }
        }

        // Selected broker from census
        protected void brokerRadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs args) {
            var ui = (RadAutoCompleteBox)sender;

            var broker = ui.SelectedItem as DatCompanyViewModel;
            if (broker != null) {
                Model.Load.Company = broker.Name;
                Model.Load.McNumber = broker.McNumber;
                Model.Load.DatBrokerId = broker.EntityId;
                Model.Load.Contacts = broker.Contacts;
            }
        }

        // Selected customer
        protected void customerRadAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs args) {
            var ui = (RadAutoCompleteBox)sender;

            var item = ui.SelectedItem as CustomerCompanyViewModel;
            if (item != null) {
                Model.Load.CustomerId = item.EntityId;
                Model.Load.Company = item.Name;
                Model.Load.Contacts = item.Contacts;
            }
        }

        #region [ Sync Location ]

        private class SyncLocationClass : IDisposable {
            private Action<string, string, LocationViewModel> _dispose;

            public string City { get; }
            public string Address { get; }
            public LocationViewModel Location { get; }

            public SyncLocationClass(string city, string address, LocationViewModel location, Action<string, string, LocationViewModel> dispose) {
                this.City = city;
                this.Address = address;
                this.Location = location;
                this._dispose = dispose;
            }

            public void Dispose() {
                this._dispose(this.City, this.Address, this.Location);
            }
        }

        /// <summary>
        ///     A Dispose Method try to sync Origins </summary>
        private IDisposable SyncOrigin() {
            return new SyncLocationClass(Model.Load.OriginCity, Model.Load.OriginAddress, Model.Load.OriginLocation, (city, address, location) => {
                var targetLoads = Model.Root.Loads.Select(e => e.Load).ToList();
                targetLoads.Add(Model.Root.Load);
                targetLoads.Remove(Model.Load);

                foreach (var load in targetLoads) {
                    if (load.OriginCity == city && 
                        load.OriginAddress == address &&
                        load.OriginLocation != null && 
                        load.OriginLocation.CompareTo(location) == 0) {
                        load.OriginCity = Model.Load.OriginCity;
                        load.OriginAddress = Model.Load.OriginAddress;
                        load.OriginLocation = (LocationViewModel)Model.Load.OriginLocation.Clone();
                    }
                }
            });
        }

        /// <summary>
        ///     A Dispose Method try to sync Destinations </summary>
        private IDisposable SyncDestination() {
            return new SyncLocationClass(Model.Load.DestinationCity, Model.Load.DestinationAddress, Model.Load.DestinationLocation, (city, address, location) => {
                var targetLoads = Model.Root.Loads.Select(e => e.Load).ToList();
                targetLoads.Add(Model.Root.Load);
                targetLoads.Remove(Model.Load);

                foreach (var load in targetLoads) {
                    if (load.DestinationCity == city &&
                        load.DestinationAddress == address && 
                        load.DestinationLocation != null && 
                        load.DestinationLocation.CompareTo(location) == 0) {
                        load.DestinationCity = Model.Load.DestinationCity;
                        load.DestinationAddress = Model.Load.DestinationAddress;
                        load.DestinationLocation = (LocationViewModel)Model.Load.DestinationLocation.Clone();
                    }
                }
            });
        }

        #endregion [ Sync Location ]

        #endregion [ AutoCompleter ]

        #region [ Distance ]

        private async void CalcDistance() {
            if (Model.Load.OriginLocation != null && Model.Load.DestinationLocation != null) {
                Model.Load.DistanceLoading = true;
                _errorTextBlock.Text = null;
                var result = await _helperService.GetDistance(LocationConverter.ToDto(Model.Load.OriginLocation), LocationConverter.ToDto(Model.Load.DestinationLocation));
                if (result.IsSuccessed) {
                    Model.Load.Distance = (int)DistanceHelper.KilometersToMiles(result.Distance / 1000);
                } else {
                    _errorTextBlock.Text = result.ErrorMessage;
                    Model.Load.Distance = null;
                }
                Model.Load.DistanceLoading = false;
            } else {
                Model.Load.Distance = null;
            }
        }

        #endregion [ Distance ]

        #region [ Deviding ]

        protected void btnSplit_OnClicked(object sender, RoutedEventArgs e) {
            // https://www.telerik.com/forums/radnumericupdown-does-not-fire-propertychanged-when-empty
            // <telerik:RadNumericUpDown Value="{Binding .. UpdateSourceTrigger=PropertyChanged}" does not worked
            Keyboard.ClearFocus();
            var load = Model.Load;
            if (this.CheckPalletised(load) == false) return;
            var dialog = new LoadDivideDialog(_dispatchService, (LoadViewModel)load.Clone(),
                Model.Root.Load != load ? (LoadViewModel)Model.Root.Load.Clone() : null);
            if (dialog.ShowDialog() == true) {
                SetChildren(dialog.Result.InitialLoad, dialog.Result.Load1, dialog.Result.Load2);
                EditBaseLoadControl.SetExtraIndex(Model.Root.Loads, Model.Load.Index);
                //dialog.Model = null;
            }
        }

        /// <summary>
        ///     Similars EditLoadDialog.xaml.cs -> AddSubload_Clicked </summary>
        protected void AddSubload_Clicked(object sender, RoutedEventArgs e) {
            var load = new LoadViewModel();
            load.Index = Model.Load.Index;
            load.RecordId = Model.Load.RecordId;
            load.ActualCompanyId = Model.Load.ActualCompanyId;
            load.Equipment = Model.Load.Equipment;
            load.PalletType = Model.Load.PalletType;
            load.LoadType = Model.Load.LoadType;
            load.BookedDate = Model.Load.BookedDate;
            load.BoardChanged = Model.Load.BoardChanged;
            load.Foreground = Model.Load.Foreground;
            load.Background = Model.Load.Background;
            load.ParentId = Model.Load.EntityId;
            load.TrailerType = Model.Load.TrailerType;
            load.Temperature = Model.Load.Temperature;
            load.BoardType = Model.Load.BoardType;

            var wrapper = new WrapperLoadViewModel(load);

            Model.Root.Loads.Add(wrapper);
            Model.Root.Load.ContainerType = ContainerType.Quantity;

            EditBaseLoadControl.SetExtraIndex(Model.Root.Loads, Model.Load.Index);
        }

        private bool CheckPalletised(LoadViewModel load) {
            if (load.Pallets != null) return true;
            if (MessageBox.Show("it this load palletised?", "Split Load", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                MessageBoxEx.Warning("Please enter pallet count!");
                return false;
            } else {
                return true;
            }
        }

        private void SetChildren(LoadViewModel root, LoadViewModel load1, LoadViewModel load2) {
            // assigns root or subload
            if (Model.Load.ParentId == null) {
                Model.Load.Assign(root);
                Model.Root.Loads.Add(new WrapperLoadViewModel(load1));
                Model.Root.Loads.Add(new WrapperLoadViewModel(load2));
            } else {
                if (load1.EntityId != Model.Load.EntityId) {
                    Model.Root.Loads.Add(new WrapperLoadViewModel(load1));
                } else {
                    // saves index
                    var index = Model.Load.Index;
                    var indexExtra = Model.Load.IndexExtra;

                    Model.Load.Assign(load1);

                    Model.Load.Index = index;
                    Model.Load.IndexExtra = indexExtra;
                }
                if (load2.EntityId != Model.Load.EntityId) {
                    Model.Root.Loads.Add(new WrapperLoadViewModel(load2));
                } else {
                    // saves index
                    var index = Model.Load.Index;
                    var indexExtra = Model.Load.IndexExtra;

                    Model.Load.Assign(load2);

                    Model.Load.Index = index;
                    Model.Load.IndexExtra = indexExtra;
                }
            }
        }

        public static void SetExtraIndex(IList<WrapperLoadViewModel> loads, int? index) {
            loads = loads.Where(e => e.Load.IsDeleted == false).ToList();
            var indexExtra = 1;
            foreach (var load in loads) {
                load.Load.Index = index;
                if (load.Load.IsDeleted == false) {
                    load.Load.IndexExtra = $"/{indexExtra}";
                    indexExtra++;
                } else {
                    load.Load.IndexExtra = null;
                }
            }
        }

        #endregion [ Deviding ]

        protected void TrailerScheme_OnClick(object sender, RoutedEventArgs e) {
            var load = Model.Load;
            if (load.TrailerSchemeId != null) {
                FileLoad(new FileViewModel() {
                    EntityId = (Guid)load.TrailerSchemeId,
                    Name = load.TrailerSchemeFile
                });
            } else {
                _l.e($"A Load<{load.EntityId}> doesn't have TrailerSchemeId");
            }
        }

        #region open

        private void FileLoad(FileViewModel fileViewModel) {
            App.Services.GetRequiredService<DispatchService>().FileLoad(fileViewModel, FileLoadCompeleted);
            //loading.Visibility = Visibility.Visible;
        }

        private async Task FileLoadCompeleted(ViewModelWrapper<FileViewModel> wrapper) {
            if (Dispatcher.CheckAccess() == false) {
                await Dispatcher.InvokeAsync(async () => await FileLoadCompeleted(wrapper));
                return;
            }
            //loading.Visibility = Visibility.Collapsed;
            if (wrapper.Status == ResponseStatus.Success) {
                try {
                    var path = Path.Combine(Path.GetTempPath(), wrapper.Data.EntityId.ToString());
                    if (Directory.Exists(path) == false) {
                        Directory.CreateDirectory(path);
                    }
                    var fullPath = Path.Combine(path, wrapper.Data.Name);
                    File.WriteAllBytes(fullPath, wrapper.Data.Data);
                    var process = new Process {
                        EnableRaisingEvents = true,
                        StartInfo = new ProcessStartInfo(fullPath) { WorkingDirectory = path }
                    };
                    process.Start();
                } catch (Exception exception) {
                    MessageBoxEx.Error(exception.Message);
                }
            } else {
                MessageBoxEx.Error(wrapper.MessageError);
            }
        }

        #endregion open
    }
}
