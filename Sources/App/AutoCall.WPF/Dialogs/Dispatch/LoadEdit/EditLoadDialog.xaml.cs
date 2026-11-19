using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Services;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoCall.WPF.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.DataServices;
using AutoDispatch.Logic.Services;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Telerik.Windows.Controls;
using AutoCall.Common.Collections;
using System.Windows.Input;
using AutoDispatch.Data;
using System.Threading.Tasks;
using AutoCall.Logic.ViewModels;
using AutoDispatch.Logic.DataServices.Requests;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.DataServices.Responses;
using System.Diagnostics;

namespace AutoCall.WPF.Dialogs.Dispatch {

    public partial class EditLoadDialog : BaseDispatchDialog {

        public bool IsReadOnly { get; set; }

        public double TabWidth = 600;

        public bool Ok { get; set; }

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        /// <summary>
        ///     Call service instance </summary>
        protected ICallService _callService;

        // мёртвый код V1: IApiService.GetHybridRate (DAT rate) — сервер V1 Dispatch/GetHybridRate всегда бросал «DISABLED», на сервер V3 не перенесён
        //private IApiService _apiService;
        private DatCompanyDataService _datCompanyService;
        private DispatchService _dispatchService;
        private IMapper _mapper;

        public EditBookLoadViewModel Model {
            get { return DataContext as EditBookLoadViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Using if owner edits ActualCompany </summary>
        private Guid? _originActualCompanyId = null;

        private void Expander_OnExpanded(object sender, RoutedEventArgs args) {
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            base.Width = this.TabWidth * 2;
        }

        private void Expander_OnCollapsed(object sender, RoutedEventArgs args) {
            if (Model.Loads.Any(e => e.Load.Status != LoadStatus.None && e.Load.TrailerId != null && e.Load.HistoryTrailerId != null)) {
                MessageBoxEx.Error(this, "loads cannot be removed if at least one has been assigned");
                return;
            }

            if (rootGrid.ColumnDefinitions.Count > 1) {
                rootGrid.ColumnDefinitions.RemoveAt(1);
                base.Width = this.TabWidth;

                Model.Loads.ForEach(e => e.Load.IsDeleted = true);
                Model.Load.ContainerType = ContainerType.None;
            }
        }

        private void Delete_MouseLeftButtonUp(object sender, MouseButtonEventArgs args) {
            var load = ((WrapperLoadViewModel)((Image)sender).DataContext).Load;
            if (MessageBox.Show($"Are you sure you want to delete load '{load.IndexString}'", "Delete load",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            load.IsDeleted = true;
            if (Model.Loads.All(e => e.Load.IsDeleted == true)) {
                Model.Load.ContainerType = ContainerType.None;
            }
        }

        private void Up_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var wrapper = (WrapperLoadViewModel)((Image)sender).DataContext;
            var index = Model.Loads.IndexOf(wrapper);
            var newIndex = index > 0 ? index - 1 : index;
            Model.Loads.Move(index, newIndex);
            EditBaseLoadControl.SetExtraIndex(Model.Loads, Model.Load.Index);
        }

        private void Down_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var wrapper = (WrapperLoadViewModel)((Image)sender).DataContext;
            var index = Model.Loads.IndexOf(wrapper);
            var newIndex = index < Model.Loads.Count - 1 ? index + 1 : index;
            Model.Loads.Move(index, newIndex);
            EditBaseLoadControl.SetExtraIndex(Model.Loads, Model.Load.Index);
        }

        private void SubExpander_OnExpanded(object sender, RoutedEventArgs args) {
            foreach (var wrapper in Model.Loads) {
                if (wrapper != null && wrapper != ((Expander)sender).DataContext) {
                    if (wrapper.IsExpanded) {
                        wrapper.IsExpanded = false;
                    }
                }
            }
        }

        private void AddSubload_Clicked(object sender, RoutedEventArgs e) {
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

            Model.Loads.Add(wrapper);

            EditBaseLoadControl.SetExtraIndex(Model.Loads, Model.Load.Index);
            Expand(wrapper);
        }

        private void Expand(WrapperLoadViewModel wrapper) {
            foreach (var w in Model.Loads) {
                if (w != null && wrapper != w) {
                    if (w.IsExpanded) {
                        w.IsExpanded = false;
                    }
                }
            }
            wrapper.IsExpanded = true;
        }

        #region [ .ctor ]

        /// <summary>
        ///     Caching </summary>
        public EditLoadDialog() {
        }

        /// <summary>
        ///     Constructor </summary>
        public EditLoadDialog(DispatchService dispatchService, ICallService callService, IMapper mapper) {
            //_apiService = App.Services.GetRequiredService<IApiService>();
            _datCompanyService = App.Services.GetRequiredService<DatCompanyDataService>();
            _dispatchService = dispatchService;
            //_fileService = fileService;
            _mapper = mapper;
            _callService = callService;

            InitializeComponent();

            base.Width = this.TabWidth;
        }

        // V1: конструкторы «груз из записи лоадборда» (DatPower, CHRWTrucks, JBHunt, Truckstop, Knight, EnglandLogistics, Bnsf, Coyote,
        // XPOLogistics, ArcBest, Truckloads) — вызывались из бордов модуля AutoCall, не перенесены

        public EditLoadDialog(DispatchService dispatchService, ICallService callService, IMapper mapper,
            LoadViewModel load,
            IList<WarehouseViewModel> warehouses,
            IList<TrailerViewModel> trailers,
            IList<TruckViewModel> trucks,
            IList<DriverViewModel> drivers,
            IList<DatCompanyViewModel> datCompanies,
            IList<DatDriverViewModel> datDrivers,
            IList<ActualCompanyViewModel> actualCompanies,
            ActualCompanyViewModel company) : this(dispatchService, callService, mapper) {
            Model = new EditBookLoadViewModel(load, actualCompanies, company) {
                Warehouses = warehouses,
                Trailers = trailers,
                Trucks = trucks,
                Drivers = drivers,
                DatCompanies = datCompanies,
                DatDrivers = datDrivers,
            };
            Title = $"Edit Booked Load #{load.IndexString} ({load.Status.GetDescription()})";
        }

        public EditLoadDialog(DispatchService dispatchService, ICallService callService, IMapper mapper,
            LoadViewModel load,
            IList<ActualCompanyViewModel> actualCompanies,
            ActualCompanyViewModel company) : this(dispatchService, callService, mapper) {
            Model = new EditBookLoadViewModel(load, actualCompanies, company);
            Title = $"Edit Booked Load #{load.IndexString} ({load.Status.GetDescription()})";
        }

        public EditLoadDialog(DispatchService dispatchService, ICallService callService, IMapper mapper,
            LoadViewModel load, List<LoadViewModel> children,
            IList<WarehouseViewModel> warehouses,
            IList<TrailerViewModel> trailers,
            IList<TruckViewModel> trucks,
            IList<DriverViewModel> drivers,
            IList<DatCompanyViewModel> datCompanies,
            IList<DatDriverViewModel> datDrivers,
            IList<ActualCompanyViewModel> actualCompanies,
            ActualCompanyViewModel company) : this(dispatchService, callService, mapper) {
            Model = new EditBookLoadViewModel(load, actualCompanies, company) {
                Loads = new ObservableCollection<WrapperLoadViewModel>(children.Select(l => new WrapperLoadViewModel(l))),
                Warehouses = warehouses,
                Trailers = trailers,
                Trucks = trucks,
                Drivers = drivers,
                DatCompanies = datCompanies,
                DatDrivers = datDrivers,
            };
            Title = $"Edit Load #{load.IndexString}";
        }

        #endregion [ .ctor ]

        private void EditChildLoadControl_OnLoaded(object sender, RoutedEventArgs e) {
            var control = (EditBaseLoadControl)sender;
            var subload = ((WrapperLoadViewModel)((FrameworkElement)control.Parent).DataContext).Load;
            control.Model = new EditBookWrapperViewModel(subload, this.Model);
            EditLoadControl_OnLoaded((EditBaseLoadControl)sender, subload);
            if (IsReadOnly) {
                OkButton.Visibility = Visibility.Collapsed;
                btnAddSubload.Visibility = Visibility.Collapsed;
                control.IsEnabled = false;
                cbActualCompany.IsEnabled = false;
                isLoads.IsEnabled = false;
            }
        }

        private void EditRootLoadControl_OnLoaded(object sender, RoutedEventArgs args) {
            var control = (EditBaseLoadControl)sender;
            // NOTE: uses this instead sender because sender.DataContext is null
            var load = ((EditBookLoadViewModel)this.DataContext).Load;
            rootLoad.DataContext = new EditBookWrapperViewModel(load, this.Model);
            EditLoadControl_OnLoaded(control, load);
            //if (IsEdit) { я не понял в каком случае нужно это условие
            if (load.ParentId == null)
                rootLoad.FilesLoad();
            //}
            if (IsReadOnly) {
                control.IsEnabled = false;
                OkButton.Visibility = Visibility.Collapsed;
            }
        }

        private void EditLoadControl_OnLoaded(EditBaseLoadControl control, LoadViewModel load) {
            if (load.ActualCompanyId == null) {
                cbActualCompany.IsEnabled = true;
            }
            // Owner can edit the actual company. Search tag "EditActualCompany" in AutoCall.App and AutoCall.Web projects
            var session = UserSession.Current;
            // Edited: all users have a permission to change a dispatch company
            /*if (RolesExtension.CheckPermission((RoleName)session.Roles, RoleName.Owner))*/
            {
                cbActualCompany.IsEnabled = true;
                _originActualCompanyId = load.ActualCompanyId;
            }

            control.Init(_dispatchService, _callService, ErrorTextBlock);
            if (load.OriginLocation == null && String.IsNullOrEmpty(load.OriginCity) == false) {
                var autoComplete = (RadAutoCompleteBox)control.FindName("originAutoComplete");
                autoComplete.Focus();
                this.Focus();
            }
            if (load.DestinationLocation == null && String.IsNullOrEmpty(load.DestinationCity) == false) {
                var autoComplete = (RadAutoCompleteBox)control.FindName("destinationAutoComplete");
                autoComplete.Focus();
                this.Focus();
            }
        }

        private void SubComboBox_Loaded(object sender, RoutedEventArgs e) {
            if (((WrapperLoadViewModel)((RadComboBox)sender).DataContext).Load.ActualCompanyId == null) {
                ((RadComboBox)sender).IsEnabled = true;
            }
        }

        private void SetRootOriginDesctination() {
            var root = Model.Load;
            var loads = Model.Loads.Where(e => e.Load != null && e.Load.IsDeleted == false);
            if (loads.Any()) {
                var first = loads.First().Load;
                var last = loads.Last().Load;
                root.PickupDate = first.PickupDate;
                root.PickupFrom = first.PickupFrom;
                root.PickupTo = first.PickupTo;
                root.DeliveryDate = last.DeliveryDate;
                root.DeliveryFrom = last.DeliveryFrom;
                root.DeliveryTo = last.DeliveryTo;
                root.ShipperId = first.ShipperId;
                root.ShipperCompany = first.ShipperCompany;
                root.ShipperContacts = first.ShipperContacts;
                root.ReceiverId = last.ReceiverId;
                root.ReceiverCompany = last.ReceiverCompany;
                root.ReceiverContacts = last.ReceiverContacts;
                root.OriginCity = first.OriginCity;
                root.OriginAddress = first.OriginAddress;
                root.OriginLocation = (LocationViewModel)first.OriginLocation?.Clone();
                root.DestinationCity = last.DestinationCity;
                root.DestinationAddress = last.DestinationAddress;
                root.DestinationLocation = (LocationViewModel)last.DestinationLocation?.Clone();
            }
            root.Distance = null;
        }

        private async void btnOk_Clicked(object sender, RoutedEventArgs args) {
            var allLoads = Model.Loads.ToList();
            var activeLoads = allLoads.Where(e => e.Load.IsDeleted == false).ToList();
            if (Model.Load.ContainerType == ContainerType.Quantity && activeLoads.Any()) {
                SetRootOriginDesctination();
            }

            if (Model.Load.ActualCompanyId == null) {
                MessageBoxEx.Alert(this, "Please select a Dispatch Company");
                return;
            }
            if (activeLoads.Count(e => e.Load.IsDeleted == false) == 1) {
                MessageBoxEx.Alert(this, "You can't save the cargo with one part");
                return;
            }
            if (Model.Load.PickupDate < new DateTime(2000, 1, 1)) {
                MessageBoxEx.Error(this, "Please enter a valid Pick-up Date");
                return;
            }
            if (Model.Load.DeliveryDate < new DateTime(2000, 1, 1)) {
                MessageBoxEx.Error(this, "Please enter a valid Delivery Date");
                return;
            }
            if (Model.Load.PickupDate.Date + Model.Load.PickupTo > Model.Load.DeliveryDate.Date + Model.Load.DeliveryFrom) {
                MessageBoxEx.Error(this, $"pick-up date must less than delivery date\npick-up:{Model.Load.PickupDate.Date + Model.Load.PickupTo:d/M h:mm tt}\ndelivery:{Model.Load.DeliveryDate.Date + Model.Load.DeliveryFrom:d/M h:mm tt}");
                return;
            }
            foreach (WrapperLoadViewModel wrapper in activeLoads) {
                if (wrapper.Load.PickupDate.Date + wrapper.Load.PickupTo > wrapper.Load.DeliveryDate.Date + wrapper.Load.DeliveryFrom) {
                    MessageBoxEx.Error(this, $"pick-up date must less than delivery date\npick-up:{wrapper.Load.PickupDate.Date + wrapper.Load.PickupTo:d/M h:mm tt}\ndelivery:{wrapper.Load.DeliveryDate.Date + wrapper.Load.DeliveryFrom:d/M h:mm tt}");
                    return;
                }
                if (wrapper.Load.ActualCompanyId == null) {
                    MessageBoxEx.Alert(this, "Please select a Dispatch Company");
                    return;
                }
            }
            if (Model.Load.OriginLocation == null) {
                MessageBoxEx.Alert(this, "Please enter a Origin\nExample: Chicago, IL");
                return;
            }
            if (Model.Load.DestinationLocation == null) {
                MessageBoxEx.Alert(this, "Please enter a Destination\nExample: Chicago, IL");
                return;
            }

            if (Model.Load.ContainerType != ContainerType.None) {
                foreach (var load in activeLoads) {
                    if (load.Load.OriginLocation == null) {
                        MessageBoxEx.Alert(this, "Please enter an Origin\nExample: Chicago, IL");
                        return;
                    }
                    if (load.Load.DestinationLocation == null) {
                        MessageBoxEx.Alert(this, "Please enter a Destination\nExample: Chicago, IL");
                        return;
                    }
                }
            }

            if (Model.Load.BoardType == DispatchBoardType.Carrier && String.IsNullOrEmpty(Model.Load.Company)) {
                MessageBoxEx.Error("Please enter a Broker");
                return;
            }

            if ((Model.Load.Weight == null || Model.Load.Weight > 150) && Model.Load.WeightType == AutoDispatch.Logic.Enums.WeightType.kLbs) {
                MessageBoxEx.Alert(this, "Please enter a valid Weight\nExample: 45 kLbs");
                return;
            }


            if ((Model.Load.Weight == null || Model.Load.Weight > 150000) && Model.Load.WeightType == AutoDispatch.Logic.Enums.WeightType.Lbs) {
                MessageBoxEx.Alert(this, "Please enter a valid Weight\nExample: 45000 Lbs");
                return;
            }

            LoadViewModel exists;
            if (String.IsNullOrEmpty(Model.Load.RecordId) == false && (exists = DispatchBoardViewModel.Instance.Loads.FirstOrDefault(l => Model.Load.EntityId != l.EntityId && Model.Load.EntityId != l.ParentId && l.RecordId == Model.Load.RecordId && l.ActualCompanyId == Model.Load.ActualCompanyId)) != null) {
                MessageBox.Show($"Pro# '{Model.Load.RecordId}' already exists\nLoad#: {exists.IndexString}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // checks by simple load
            if (activeLoads.Any() == false) {
                Model.Load.ContainerType = ContainerType.None;
                Model.Load.DivideType = DivideType.None;
            }

            if (Model.Load.ContainerType == ContainerType.Quantity) {
                var content = String.Empty;
                if (activeLoads.Any()) {
                    int? weight = activeLoads.Sum(l => l.Load.Weight);
                    if (weight != null && weight != 0 && Model.Load.Weight != null && Model.Load.Weight != 0 &&
                        weight != Model.Load.Weight) {
                        content += $"Value of Weight doesn't match: {Model.Load.Weight} != {weight} ({String.Join("+", activeLoads.Select(l => l.Load.Weight ?? 0))})\n";
                    }
                    int? length = activeLoads.Sum(l => l.Load.Length);
                    if (length != null && length != 0 && Model.Load.Length != null && Model.Load.Length != 0 &&
                        length != Model.Load.Length) {
                        content += $"Value of Length doesn't match: {Model.Load.Length} != {length} ({String.Join("+", activeLoads.Select(l => l.Load.Length ?? 0))})\n";
                    }
                    int? pallets = activeLoads.Sum(l => l.Load.Pallets);
                    if (pallets != null && pallets != 0 && Model.Load.Pallets != null && Model.Load.Pallets != 0 &&
                        pallets != Model.Load.Pallets) {
                        content += $"Value of Pallets doesn't match: {Model.Load.Pallets} != {pallets} ({String.Join("+", activeLoads.Select(l => l.Load.Pallets ?? 0))})\n";
                    }
                    var lineHauls = activeLoads.Sum(l => l.Load.LineHaul);
                    if (lineHauls != null && lineHauls != 0 && Model.Load.LineHaul != null && Model.Load.LineHaul != 0 &&
                        lineHauls != Model.Load.LineHaul) {
                        content += $"Value of Line Haul doesn't match: {Model.Load.LineHaul} != {lineHauls} ({String.Join("+", activeLoads.Select(l => l.Load.LineHaul ?? 0))})\n";
                    }
                }
                if (String.IsNullOrEmpty(content) == false) {
                    if (MessageBox.Show(content, "Save loads", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                        return;
                    }
                }
            }

            if (Model.Load.ContainerType == ContainerType.Distance) {
                var content = String.Empty;
                if (activeLoads.Any()) {
                    foreach (WrapperLoadViewModel wrapper in activeLoads) {
                        if (Nullable.Equals(Model.Load.Weight, Model.Load.Weight) == false) {
                            content += $"Value of Weight doesn't match: {Model.Load.Weight} != {wrapper.Load.Weight}\n";
                            break;
                        }
                    }
                    foreach (WrapperLoadViewModel wrapper in activeLoads) {
                        if (Nullable.Equals(Model.Load.Length, Model.Load.Length) == false) {
                            content += $"Value of Length doesn't match: {Model.Load.Length} != {wrapper.Load.Length}\n";
                            break;
                        }
                    }
                    foreach (WrapperLoadViewModel wrapper in activeLoads) {
                        if (Nullable.Equals(Model.Load.Pallets, Model.Load.Pallets) == false) {
                            content += $"Value of Pallets doesn't match: {Model.Load.Pallets} != {wrapper.Load.Pallets}\n";
                            break;
                        }
                    }
                }
                if (String.IsNullOrEmpty(content) == false) {
                    if (MessageBox.Show(content + "\nAre you sure you want to continue?", "Save loads", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                        return;
                    }
                }
            }

            if (Model.Load.ContainerType != ContainerType.None) {
                var content = String.Empty;
                if (activeLoads.Any()) {
                    if (activeLoads.Any(l => l.Load.OriginCity == Model.Load.OriginCity)) {
                        if (activeLoads.Any(l => l.Load.OriginCity == Model.Load.OriginCity &&
                                                 l.Load.OriginAddress == Model.Load.OriginAddress)) {
                            if (activeLoads.Any(l => l.Load.OriginCity == Model.Load.OriginCity &&
                                                     l.Load.OriginAddress == Model.Load.OriginAddress &&
                                                     l.Load.PickupFullDateTime == Model.Load.PickupFullDateTime)) {
                                if (activeLoads.Any(l => l.Load.OriginCity == Model.Load.OriginCity &&
                                                         l.Load.OriginAddress == Model.Load.OriginAddress &&
                                                         l.Load.PickupFullDateTime == Model.Load.PickupFullDateTime &&
                                                         l.Load.ShipperCompany == Model.Load.ShipperCompany)) {

                                } else {
                                    content += $"Shipments do not contain 'Shipper Company' equal {Model.Load.ShipperCompany}\n";
                                }
                            } else {
                                content += $"Shipments do not contain 'Pickup Date' equal {Model.Load.PickupFullDateTime}\n";
                            }
                        } else {
                            content += $"Shipments do not contain 'Origin Address' equal {Model.Load.OriginAddress}\n";
                        }
                    } else {
                        content += $"Shipments do not contain 'Origin City' equal {Model.Load.OriginCity}\n";
                    }

                    if (activeLoads.Any(l => l.Load.DestinationCity == Model.Load.DestinationCity)) {
                        if (activeLoads.Any(l => l.Load.DestinationCity == Model.Load.DestinationCity &&
                                                 l.Load.DestinationAddress == Model.Load.DestinationAddress)) {
                            if (activeLoads.Any(l => l.Load.DestinationCity == Model.Load.DestinationCity &&
                                                     l.Load.DestinationAddress == Model.Load.DestinationAddress &&
                                                     l.Load.DeliveryFullDateTime == Model.Load.DeliveryFullDateTime)) {
                                if (activeLoads.Any(l => l.Load.DestinationCity == Model.Load.DestinationCity &&
                                                         l.Load.DestinationAddress == Model.Load.DestinationAddress &&
                                                         l.Load.DeliveryFullDateTime == Model.Load.DeliveryFullDateTime &&
                                                         l.Load.ReceiverCompany == Model.Load.ReceiverCompany)) {

                                } else {
                                    content += $"Shipments do not contain 'Receiver Company' equal {Model.Load.ReceiverCompany}\n";
                                }
                            } else {
                                content += $"Shipments do not contain 'Delivery Date' equal {Model.Load.DeliveryFullDateTime}\n";
                            }
                        } else {
                            content += $"Shipments do not contain 'Destination Address' equal {Model.Load.DestinationAddress}\n";
                        }
                    } else {
                        content += $"Shipments do not contain 'Destination City' equal {Model.Load.DestinationCity}\n";
                    }
                }
                if (String.IsNullOrEmpty(content) == false) {
                    if (MessageBox.Show(content + "\nAre you sure you want to continue?", "Save loads", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                        return;
                    }
                }
            }

            if (Model.Load.ContainerType == ContainerType.None && activeLoads.Any(e => e.Load.IsDeleted == false)) {
                MessageBox.Show("An error has occurred. Re-create cargo", "Save loads", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var divideType = AutoDispatch.Logic.Helpers.DispatchHelper.GetDivideType(Model.Load);
            if (divideType == DivideType.None) {
                var result = AutoCall.WPF.Helpers.DispatchHelper.AskDivideType(this);
                if (result == DivideType.None) {
                    return;
                } else {
                    divideType = result;
                }
            }
            Model.Load.DivideType = divideType;

            var saveList = new List<LoadViewModel>() { Model.Load };
            foreach (var wrapper in allLoads) {
                // NOTE: Skips shipments, but subloads have properties from a root
                if (Model.Load.ContainerType != ContainerType.Shipment) {
                    AutoDispatch.Logic.Helpers.DispatchHelper.CopyContainerProperties(Model.Load, wrapper.Load);
                }
                saveList.Add(wrapper.Load);
            }

            // DAT Rate
            // мёртвый код V1: сервер V1 Dispatch/GetHybridRate всегда бросал «DISABLED» (result.IsSuccessed == false — DatRate не заполнялся),
            // DatRateDataService на сервер V3 не перенесён; GetRateRequest/RateConverter/DatPowerRateViewModel — модуль AutoCall
            //if (Model.Load.ParentId == null && Model.Load.DatRate == null) {
            //    var origin = new AutoCall.Logic.Dto.LocationDto() {
            //        Lat = Model.Load.OriginLocation.Latitude,
            //        Long = Model.Load.OriginLocation.Longitude
            //    };
            //    var destination = new AutoCall.Logic.Dto.LocationDto() {
            //        Lat = Model.Load.DestinationLocation.Latitude,
            //        Long = Model.Load.DestinationLocation.Longitude
            //    };
            //    var request = new GetRateRequest() {
            //        Origin = origin,
            //        Destination = destination,
            //        AutoCallEquipment = Model.Load.Equipment,
            //        Board = Logic.Contracts.Enums.DatPowerBoardType.PostTruck
            //    };
            //    base.BusyStart();
            //    var result = await Task.Factory.StartNew(() => _apiService.GetHybridRate(request));
            //    base.BusyFinish();

            //    var rateModel = new DatPowerRateViewModel() { Data = new DatPowerRateDataViewModel() };
            //    if (result.IsSuccessed) {
            //        var model = rateModel.Data;
            //        Converters.DatPower.Rate.RateConverter.Convert(result, model);
            //        model.Calc();
            //        Model.Load.DatRate = (decimal?)(model.ShowRates ? model.SpotEstimatedLinehaulRateFuel * model.RateMiles : model.SpotEstimatedLinehaulTotal);
            //    }
            //}

            if (saveList.Any()) {
                // Tag: "EditActualCompany"
                if (_originActualCompanyId != null &&
                    _originActualCompanyId != Model.Load.ActualCompanyId &&
                    Model.Load.Index != null) {
                    foreach (LoadViewModel load in saveList) {
                        // Change Load#
                        load.IndexCorrected = true;
                    }
                }
                // saves
                _dispatchService.SaveLoads(saveList, this.SaveLoadsCompleted);
                // save files
                if (Model.Load.Index == null) {
                    foreach (FileViewModel file in Model.Load.Files.Where(e => e.LazySave))
                        _dispatchService.FileSave(file, async (w) => { });
                }
            }
            base.BusyStart();
        }

        private async Task SaveLoadsCompleted(ViewModelWrapper<List<LoadViewModel>> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                // NOTES: если открыть после сохранения, событие бывает не успело прилететь и видим груз без внесённых изменений
                //await Task.Delay(TimeSpan.FromSeconds(1)); использую wrapper.Load.Saving = true;
                await Dispatcher.InvokeAsync(async () => await SaveLoadsCompleted(wrapper));
                return;
            }
            await base.SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                // not async
                Model.Load = wrapper.Data?.FirstOrDefault(l => l.ParentId == null && l.ShipmentId == null);
                // Adds confirmation
                if (Model.Load != null && Model.Load.ConfirmationFile == null && Model.Load.BoardType == DispatchBoardType.Carrier) {
                    // attached in file dialog
                    var load = Model.Load;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Dispatcher.InvokeAsync(() => {
                    var confDlg = new FileConfirmationDialog(_dispatchService, true) { Model = load };
                    //confDlg.Owner = this;
                        if (confDlg.ShowDialog() == true) {
                        }
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                Ok = true;
                Close();
            }
        }

        private async void SaveDatCompanyCompleted(ViewModelWrapper<DatCompanyViewModel> wrapper) {
            if (this.Dispatcher.CheckAccess() == false) {
                // NOTES: если открыть после сохранения, событие бывает не успело прилететь и видим груз без внесённых изменений
                //await Task.Delay(TimeSpan.FromSeconds(1)); использую wrapper.Load.Saving = true;
                Dispatcher.Invoke(() => SaveDatCompanyCompleted(wrapper));
                return;
            }
            if (wrapper.Status == ResponseStatus.Success) {
                MessageBoxEx.Info(this, $"Broker '{wrapper.Data.Name}' was added");
            }
            await base.SaveCompleted(wrapper, false);
            if (wrapper.Status == ResponseStatus.Success) {
                Ok = true;
                Close();
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e) {
            Ok = false;
            Close();
        }

        private void cbActualCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_originActualCompanyId != null) {
                foreach (var subLoad in this.Model.Loads) {
                    subLoad.Load.ActualCompanyId = this.Model.Load.ActualCompanyId;
                }
            }
        }
    }
}
