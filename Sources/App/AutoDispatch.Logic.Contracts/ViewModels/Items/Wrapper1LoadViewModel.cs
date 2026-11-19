using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Attributes;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class Wrapper1LoadViewModel : ViewModel, IComparable {
        private LoadViewModel _load;
        //private DatCompanyViewModel _brokerCompany;
        private DriverViewModel _driver;
        private TruckViewModel _truck;
        private TrailerViewModel _trailer;
        private TrailerViewModel _assignedTrailer;
        private WarehouseViewModel _destinationWarehouse;
        private WarehouseViewModel _warehouse;
        private YardViewModel _yard;
        private DatCompanyViewModel _datCompany;
        private DatDriverViewModel _datDriver;
        private LoadViewModel _shipment;
        private DriverViewModel _historyDriver;
        private TruckViewModel _historyTruck;
        private TrailerViewModel _historyTrailer;
        private WarehouseViewModel _historyWarehouse;
        private DatCompanyViewModel _historyDatCompany;
        private DatDriverViewModel _historyDatDriver;
        private LoadViewModel _historyShipment;
        private ActualCompanyViewModel _actualCompany;
        private SetupPacketViewModel _setupPacket;
        private Wrapper1LoadViewModel _linkedWrapper;
        private int _linkedWrapperCount;
        private ObservableCollection<Wrapper1LoadViewModel> _children = new ObservableCollection<Wrapper1LoadViewModel>();
        private bool isExpanded;

        public Wrapper1LoadViewModel() {
        }
        public Wrapper1LoadViewModel(LoadViewModel load) {
            this.Load = load;
        }

        public LoadViewModel Load {
            get { return _load; }
            set {
                if (_load != value) {
                    _load = value;
                    OnPropertyChanged();
                }
            }
        }

        public DriverViewModel Driver {
            get { return _driver; }
            set {
                if (_driver != value) {
                    _driver = value;
                    OnPropertyChanged();
                }
            }
        }

        public TruckViewModel Truck {
            get { return _truck; }
            set {
                if (_truck != value) {
                    _truck = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel Trailer {
            get { return _trailer; }
            set {
                if (_trailer != value) {
                    _trailer = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel HistoryAssignedTrailer {
            get { return _assignedTrailer; }
            set {
                if (_assignedTrailer != value) {
                    _assignedTrailer = value;
                    OnPropertyChanged();
                }
            }
        }

        public WarehouseViewModel DestinationWarehouse {
            get { return _destinationWarehouse; }
            set {
                if (_destinationWarehouse != value) {
                    _destinationWarehouse = value;
                    OnPropertyChanged();
                }
            }
        }

        public WarehouseViewModel Warehouse {
            get { return _warehouse; }
            set {
                if (_warehouse != value) {
                    _warehouse = value;
                    OnPropertyChanged();
                }
            }
        }

        public YardViewModel Yard {
            get { return _yard; }
            set {
                if (_yard != value) {
                    _yard = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Carrier </summary>
        public DatCompanyViewModel DatCompany {
            get { return _datCompany; }
            set {
                if (_datCompany != value) {
                    _datCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Carrier in Children </summary>
        /// <remarks>
        ///     Текущий wrapper может не иметь трейлер, но все дети имеют. Тогда возвращаем первый 
        ///     Используется, если кликаешь на Container, дети которго на одном трейлере
        ///     похож на DispatchHelper.GetTrailerWrapper и GetCarrierWrapper
        /// </remarks>
        public DatCompanyViewModel DatCompanyView {
            get {
                if (DatCompany != null) {
                    return DatCompany;
                }
                if (Children.All(e => e.DatCompany != null)) {
                    var datCompanies = Children.Select(e => e.DatCompany).Distinct();
                    if (datCompanies.Count() == 1) {
                        return datCompanies.First();
                    }
                }
                return null;
            }
        }

        public DatDriverViewModel DatDriver {
            get { return _datDriver; }
            set {
                if (_datDriver != value) {
                    _datDriver = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoadViewModel Shipment {
            get { return _shipment; }
            set {
                if (_shipment != value) {
                    _shipment = value;
                    OnPropertyChanged();
                }
            }
        }

        public DriverViewModel HistoryDriver {
            get { return _historyDriver; }
            set {
                if (_historyDriver != value) {
                    _historyDriver = value;
                    OnPropertyChanged();
                }
            }
        }

        public TruckViewModel HistoryTruck {
            get { return _historyTruck; }
            set {
                if (_historyTruck != value) {
                    _historyTruck = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel HistoryTrailer {
            get { return _historyTrailer; }
            set {
                if (_historyTrailer != value) {
                    _historyTrailer = value;
                    OnPropertyChanged();
                }
            }
        }

        public WarehouseViewModel HistoryWarehouse {
            get { return _historyWarehouse; }
            set {
                if (_historyWarehouse != value) {
                    _historyWarehouse = value;
                    OnPropertyChanged();
                }
            }
        }

        public DatCompanyViewModel HistoryDatCompany {
            get { return _historyDatCompany; }
            set {
                if (_historyDatCompany != value) {
                    _historyDatCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public DatDriverViewModel HistoryDatDriver {
            get { return _historyDatDriver; }
            set {
                if (_historyDatDriver != value) {
                    _historyDatDriver = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoadViewModel HistoryShipment {
            get { return _historyShipment; }
            set {
                if (_historyShipment != value) {
                    _historyShipment = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     For view model </summary>
        public int? HistoryDriverCarrier_McNumber {
            get { return HistoryDatCompany?.McNumber ?? ActualCompany?.McNumber; }
        }
        /// <summary>
        ///     For view model </summary>
        public string HistoryDriverCarrier_Name {
            get { return HistoryDatCompany?.Name ?? HistoryDriver?.DisplayName; }
        }
        /// <summary>
        ///     For view model </summary>
        public object HistoryDriverCarrier_Contacts {
            get { return (object)HistoryDatCompany?.Contacts ?? HistoryDriver?.Email; }
        }

        public ActualCompanyViewModel ActualCompany {
            get { return _actualCompany; }
            set {
                if (_actualCompany != value) {
                    _actualCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public SetupPacketViewModel SetupPacket {
            get { return _setupPacket; }
            set {
                if (_setupPacket != value) {
                    _setupPacket = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LinkedWrapperCount {
            get { return _linkedWrapperCount; }
            set {
                if (_linkedWrapperCount != value) {
                    _linkedWrapperCount = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public Wrapper1LoadViewModel LinkedWrapper {
            get { return _linkedWrapper; }
            set {
                if (_linkedWrapper != value) {
                    _linkedWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Wrapper1LoadViewModel> Children {
            get { return _children; }
            set {
                if (_children != value) {
                    _children = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HistoryAssignedTrailerNumber {
            get {
                if (Children?.Any() == false) {
                    return HistoryAssignedTrailer?.Number;
                } else {
                    return StringExtension.NullableJoin("_", Children.Select(e => e.HistoryAssignedTrailerNumber).Distinct());
                }
            }
        }

        public string TrailerNumber {
            get {
                if (Shipment != null) {
                    return $"Shipment: {Shipment.IndexString}";
                }
                if (Children?.Any() == false) {
                    return Trailer?.Number;
                } else {
                    return StringExtension.NullableJoin("_", Children.Select(e => e.TrailerNumber).Distinct());
                }
            }
        }

        public string TrailerSchemeFile {
            get {
                if (Children?.Any() == false) {
                    return Trailer?.SchemeFile;
                } else {
                    return StringExtension.NullableJoin("_", Children.Select(e => e.TrailerSchemeFile).Distinct());
                }
            }
        }

        public DateTime? TrailerSchemeDate {
            get {
                if (Children?.Any() == false) {
                    return Trailer?.SchemeDate;
                } else {
                    return Children.First().TrailerSchemeDate;
                }
            }
        }

        public string HistoryTrailerNumber {
            get {
                if (Children?.Any() == false) {
                    return HistoryTrailer?.Number ?? HistoryShipment?.IndexString;
                } else {
                    return string.Join("_", Children.Select(e => e.HistoryTrailerNumber).Distinct());
                }
            }
        }

        public string HistoryWarehouseName {
            get {
                if (Children?.Any() == false) {
                    return HistoryWarehouse?.Name;
                } else {
                    return string.Join("_", Children.Select(e => e.HistoryWarehouseName).Distinct());
                }
            }
        }

        public string LoadBindDate {
            get {
                if (Children?.Any() == false) {
                    return Load.BindDate?.ToString("d");
                } else {
                    return string.Join("_", Children.Select(e => e.Load.BindDate?.ToString("d")).Distinct());
                }
            }
        }

        public string LoadAssignedTrailerDate {
            get {
                if (Children?.Any() == false) {
                    return Load.AssignedTrailerDate?.ToString("d");
                } else {
                    return string.Join("_", Children.Select(e => e.Load.AssignedTrailerDate?.ToString("d")).Distinct());
                }
            }
        }

        public int PickupCount {
            get {
                if (Children?.Any() == false) {
                    return 1;
                } else {
                    return Children.Select(e => e.Load.OriginCity).Distinct().Count();
                }
            }
        }

        public int DeliveryCount {
            get {
                if (Children?.Any() == false) {
                    return 1;
                } else {
                    return Children.Select(e => e.Load.DestinationCity).Distinct().Count();
                }
            }
        }

        public string OriginCity {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    if (PickupCount == 1) {
                        return $"1 Pick-Up";
                    } else {
                        return $"{PickupCount} Pick-Ups";
                    }
                } else {
                    return this.Load.OriginCity;
                }
            }
        }

        public string DestinationCity {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    if (DeliveryCount == 1) {
                        return $"1 Delivery";
                    } else {
                        return $"{DeliveryCount} Deliveries";
                    }
                } else {
                    return this.Load.DestinationCity;
                }
            }
        }

        public string OriginAddress {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.OriginAddress;
                }
            }
        }

        public string DestinationAddress {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.DestinationAddress;
                }
            }
        }

        public string ShipperCompany {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.ShipperCompany;
                }
            }
        }

        public string ReceiverCompany {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.ReceiverCompany;
                }
            }
        }

        public string Pickup {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.Pickup;
                }
            }
        }

        public string Delivery {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.Delivery;
                }
            }
        }

        public DateTime? PickupDate {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.PickupDate;
                }
            }
        }

        public DateTime? DeliveryDate {
            get {
                if (Load.Container && Children?.Any() == true && this.IsExpanded == true) {
                    return null;
                } else {
                    return this.Load.DeliveryDate;
                }
            }
        }

        public bool IsExpanded {
            get => isExpanded;
            set {
                if (isExpanded != value) {
                    isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OriginCity));
                    OnPropertyChanged(nameof(DestinationCity));
                }
            }
        }

        public static List<Wrapper1LoadViewModel> GetRecursiveChildren(Wrapper1LoadViewModel wrapper) {
            var result = new List<Wrapper1LoadViewModel>();
            result.AddRange(wrapper.Children);
            foreach (var child in wrapper.Children) {
                result.AddRange(GetRecursiveChildren(child));
            }
            return result;
        }

        public bool IsNew {
            get { return Load != null && Load.IsNew; }
            set {
                if (Load != null && Load.IsNew != value) {
                    Load.IsNew = value;
                }
                // TODO delete two comments
                //if (DatCompany != null && DatCompany.IsNew != value) {
                //    DatCompany.IsNew = value;
                //}
                OnPropertyChanged();
            }
        }

        #region [ Property is valid ]

        public bool OriginIsValid {
            get {
                if (this.Children.Any() == false)
                    return true;
                return this.Children.Any(w => w.Load.OriginCity == this.Load.OriginCity && w.Load.OriginAddress == this.Load.OriginAddress);
            }
        }

        public bool DestinationIsValid {
            get {
                if (this.Children.Any() == false)
                    return true;
                return this.Children.Any(w => w.Load.DestinationCity == this.Load.DestinationCity && w.Load.DestinationAddress == this.Load.DestinationAddress);
            }
        }

        public bool PickupIsValid {
            get {
                if (this.Children.Any() == false)
                    return true;
                return this.Children.Any(w => w.Load.PickupFullDateTime == this.Load.PickupFullDateTime);
            }
        }

        public bool DeliveryIsValid {
            get {
                if (this.Children.Any() == false)
                    return true;
                return this.Children.Any(w => w.Load.DeliveryFullDateTime == this.Load.DeliveryFullDateTime);
            }
        }

        #endregion [ Property is valid ]

        public void IsNewOnChanged() {
            OnPropertyChanged("IsNew");
        }

        public void ChildrenOnChanged() {
            OnPropertyChanged("Children");
            OnPropertyChanged("Load"); // notes: необходим что бы при разбивке обновить контейнер, через Children не обновляется
        }

        public bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return
                (HistoryAssignedTrailerNumber != null && HistoryAssignedTrailerNumber.ToLower().Contains(search)) ||
                (TrailerNumber != null && TrailerNumber.ToLower().Contains(search)) ||
                (HistoryTrailerNumber != null && HistoryTrailerNumber.ToLower().Contains(search));
        }

        public int CompareTo(object obj) {
            if (Load.Index > ((Wrapper1LoadViewModel)obj).Load.Index) {
                return 1;
            }
            if (Load.Index < ((Wrapper1LoadViewModel)obj).Load.Index) {
                return -1;
            }
            return 0;
        }

        public override string ToString() {
            if (Children != null && Children.Any()) {
                return $"{Load.IndexString} ({String.Join(" ", Children.Select(e => e.Load.IndexString))})";
            } else {
                return Load.IndexString;
            }
        }
    }
}
