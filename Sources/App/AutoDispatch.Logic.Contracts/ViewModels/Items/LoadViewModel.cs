using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AutoCall.Common.Enums;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoDispatch.Logic.Enums;
using DispatchHelper = AutoCall.Common.Helpers.DispatchHelper;
using System.Collections.Generic;
using AutoMapper;
using AutoDispatch.Logic.Converters;
using AutoCall.Common;
using AutoDispatch.Dtos.Dtos;
using System.Runtime.Serialization;
using AutoDispatch.Data;
using System.Runtime.CompilerServices;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class LoadViewModel : BaseDispatchViewModel {
        private Guid? _actualCompanyId;
        private Guid? _trailerId;
        private Guid? _historyAssignedTrailerId;
        private Guid? _destinationWarehouseId;
        private Guid? _warehouseId;
        private Guid? _datCompanyId;
        private Guid? _datDriverId;
        private Guid? _historyDriverId;
        private Guid? _historyTruckId;
        private Guid? _historyTrailerId;
        private Guid? _historyWarehouseId;
        private Guid? _historyDatCompanyId;
        private Guid? _historyDatDriverId;
        private DateTime _deliveryDate;
        private TimeSpan _deliveryFrom;
        private TimeSpan _deliveryTo;
        private DateTime _pickupDate;
        private TimeSpan _pickupFrom;
        private TimeSpan _pickupTo;
        private int? _length;
        private int? _weight;
        private short? _pallets;
        private short? _spots;
        private WeightType _weightType;
        private string _commodity;
        private AutoCallEquipmentType _equipment;
        private ContactsViewModel _contacts;
        private string _company;
        private LocationViewModel _destinationLocation;
        private LocationViewModel _originLocation;
        private int? _distance;
        private string _destinationAddress;
        private string _originAddress;
        private string _destinationCity;
        private string _originCity;
        private string _comments;
        private string _comments2;
        private string _consolidation;
        private string _receiverCompany;
        private string _shipperCompany;
        private LoadType _loadType;
        private DivideType _divideType;
        private ContainerType _containerType;
        private Guid? _parentId;
        private DateTime _bookedDate;
        private DateTime _boardChanged;
        private decimal? _carrierViewTracking;
        private decimal? _lineHaul;
        private decimal? _datRate;
        private List<ExpenseViewModel> _expenses;
        private List<GainViewModel> _gains;
        private CurrencyType _currency;
        private DateTime? _quikPay;
        private int? _mcNumber;
        private long? _dotNumber;
        private Color _foreground;
        private Color _background;
        private TrailerType _trailerType;
        private short? _temperature;
        private DateTime? _bindDate;
        private DateTime? _assignedTrailerDate;
        private LoadStatus _status;
        private Guid? _customStatusId;
        private int? _index;
        private string _indexExtra;
        private string _indexPrefix;
        private byte _relativeIndex;
        private byte? _pickupOrder;
        private byte? _deliveryOrder;
        private ContactsViewModel _shipperContacts;
        private ContactsViewModel _receiverContacts;
        private DispatchBoardType _boardType;

        private DateTime? _bolDate;
        private string _bolFile;
        private DateTime? _podDate;
        private string _podFile;
        private DateTime? _lumperDate;
        private string _lumperFile;
        private DateTime? _paperworkDate;
        private string _paperworkFile;
        private DateTime? _confirmationDate;
        private string _confirmationFile;
        private DateTime? _confirmedDate;
        private string _confirmedFile;
        private List<string> _emailConfirmationSent;
        private DateTime? _emailConfirmationSentDate;

        private DateTime? _trailerSchemeDate;
        private string _trailerSchemeFile;

        private string _invoiceNumber;
        private string _invoiceFile;
        private DateTime? _invoiceDate;
        private List<string> _emailInvoiceSent;
        private DateTime? _emailInvoiceSentDate;

        private string _checkNumber;
        private string _checkFile;
        private DateTime? _checkDate;
        private float? _payout;

        private string _photoFile;

        private Guid? _oldOrderId;
        private bool _isNew;
        private bool _mark;
        private Guid? _shipmentId;
        private Guid? _historyShipmentId;
        private Wrapper1LoadViewModel _parent1;
        private Wrapper1LoadViewModel _parent2;
        private string _recordId;
        private string _bolNumber;

        private DateTime? _newScheduledAppointment;

        // Extra
        private ActualCompanyViewModel _actualCompany;
        private bool _brokerCompanyLoading;
        private bool _brokerCompanyMcLoading;
        private bool _distanceLoading;
        private bool saving;

        private DateTime? _deliveredDate;

        //public override string CreatedByName => Parent1?.Load.CreatedByName ?? base.CreatedByName;
        //public override string Created => Parent1?.Load.Created ?? base.Created;

        public string RecordId {
            get { return _recordId; }
            set {
                if (_recordId != value) {
                    _recordId = value;
                    RecordIndex = value?.OnlyNumbersAndLetters();
                    OnPropertyChanged();
                }
            }
        }

        public string BOLNumber {
            get { return _bolNumber; }
            set {
                if (_bolNumber != value) {
                    _bolNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RecordIndex { get; set; }

        public int? TrackerId {
            get => trackerId;
            set {
                if (trackerId != value) {
                    trackerId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewPhone {
            get => carrierViewPhone;
            set {
                if (carrierViewPhone != value) {
                    carrierViewPhone = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewLink {
            get => carrierViewLink;
            set {
                if (carrierViewLink != value) {
                    carrierViewLink = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? CarrierViewLastUpdatedDate {
            get => carrierViewLastUpdatedDate;
            set {
                if (carrierViewLastUpdatedDate != value) {
                    carrierViewLastUpdatedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CarrierViewLastUpdatedDateString));
                }
            }
        }
        public string CarrierViewLastUpdatedDateString => CarrierViewLastUpdatedDate != null ?
            StringExtension.ToString(DateTime.UtcNow - CarrierViewLastUpdatedDate) : null;

        public string CarrierViewStatus {
            get => carrierViewStatus;
            set {
                if (carrierViewStatus != value) {
                    carrierViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CarrierViewLate {
            get => carrierViewLate;
            set {
                if (carrierViewLate != value) {
                    carrierViewLate = value;
                    OnPropertyChanged();
                }
            }
        }
        public int? CarrierViewPickupId {
            get => carrierViewPickupId;
            set {
                if (carrierViewPickupId != value) {
                    carrierViewPickupId = value;
                    OnPropertyChanged();
                }
            }
        }
        public int? CarrierViewDeliveryId {
            get => carrierViewDeliveryId;
            set {
                if (carrierViewDeliveryId != value) {
                    carrierViewDeliveryId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewPickupComment {
            get => carrierViewPickupComment;
            set {
                if (carrierViewPickupComment != value) {
                    carrierViewPickupComment = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewDeliveryComment {
            get => carrierViewDeliveryComment;
            set {
                if (carrierViewDeliveryComment != value) {
                    carrierViewDeliveryComment = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> CarrierViewStatuses {
            get => carrierViewStatuses; set {
                if (carrierViewStatuses != value) {
                    carrierViewStatuses = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(this.CarrierViewStatuses1));
                    OnPropertyChanged(nameof(this.CarrierViewStatuses2));
                    OnPropertyChanged(nameof(this.CarrierViewStatusesString));
                    OnPropertyChanged(nameof(this.CarrierViewStatuses1String));
                    OnPropertyChanged(nameof(this.CarrierViewStatuses2String));
                }
            }
        }
        public List<string> CarrierViewStatuses1 => CarrierViewStatuses?.Where(e => e.StartsWith("tracking_") == true).ToList();
        public List<string> CarrierViewStatuses2 => CarrierViewStatuses?.Where(e => e.StartsWith("tracking_") == false).ToList();
        public string CarrierViewStatusesString => CarrierViewStatuses != null ? String.Join(", ", CarrierViewStatuses) : null;
        public string CarrierViewStatuses1String => CarrierViewStatuses1 != null ? String.Join(", ", CarrierViewStatuses1) : null;
        public string CarrierViewStatuses2String => CarrierViewStatuses2 != null ? String.Join(", ", CarrierViewStatuses2) : null;


        /// <summary>
        ///     Broker Company ID </summary>
        public Guid? ActualCompanyId {
            get { return _actualCompanyId; }
            set {
                if (_actualCompanyId != value) {
                    _actualCompanyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public ActualCompanyViewModel ActualCompany {
            get { return _actualCompany; }
            set {
                if (_actualCompany != value) {
                    _actualCompany = value;
                    _actualCompanyId = value.EntityId;
                    OnPropertyChanged();
                }
            }
        }

        public bool BrokerCompanyMcLoading {
            get { return _brokerCompanyMcLoading; }
            set {
                if (_brokerCompanyMcLoading != value) {
                    _brokerCompanyMcLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CompanyLoading {
            get { return _brokerCompanyLoading; }
            set {
                if (_brokerCompanyLoading != value) {
                    _brokerCompanyLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DistanceLoading {
            get { return _distanceLoading; }
            set {
                if (_distanceLoading != value) {
                    _distanceLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Saving {
            get => saving;
            set {
                saving = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Order ID </summary>
        public Guid? OldOrderId {
            get { return _oldOrderId; }
            set {
                if (_oldOrderId != value) {
                    _oldOrderId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? TrailerId {
            get { return _trailerId; }
            set {
                if (_trailerId != value) {
                    _trailerId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? DestinationWarehouseId {
            get { return _destinationWarehouseId; }
            set {
                if (_destinationWarehouseId != value) {
                    _destinationWarehouseId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? WarehouseId {
            get { return _warehouseId; }
            set {
                if (_warehouseId != value) {
                    _warehouseId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? DatCompanyId {
            get { return _datCompanyId; }
            set {
                if (_datCompanyId != value) {
                    _datCompanyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? DatDriverId {
            get { return _datDriverId; }
            set {
                if (_datDriverId != value) {
                    _datDriverId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryDriverId {
            get { return _historyDriverId; }
            set {
                if (_historyDriverId != value) {
                    _historyDriverId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryTruckId {
            get { return _historyTruckId; }
            set {
                if (_historyTruckId != value) {
                    _historyTruckId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryTrailerId {
            get { return _historyTrailerId; }
            set {
                if (_historyTrailerId != value) {
                    _historyTrailerId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryAssignedTrailerId {
            get { return _historyAssignedTrailerId; }
            set {
                if (_historyAssignedTrailerId != value) {
                    _historyAssignedTrailerId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryWarehouseId {
            get { return _historyWarehouseId; }
            set {
                if (_historyWarehouseId != value) {
                    _historyWarehouseId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryDatCompanyId {
            get { return _historyDatCompanyId; }
            set {
                if (_historyDatCompanyId != value) {
                    _historyDatCompanyId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryDatDriverId {
            get { return _historyDatDriverId; }
            set {
                if (_historyDatDriverId != value) {
                    _historyDatDriverId = value;
                    OnPropertyChanged();
                }
            }
        }

        // Extra
        public DatDriverViewModel DatDriverHistoryObj {
            get => datDriverHistoryObj;
            set {
                datDriverHistoryObj = value;
                OnPropertyChanged();
            }
        }

        [CompareAnyValidation]
        public string OriginCity {
            get { return _originCity; }
            set {
                if (_originCity != value) {
                    _originCity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OriginLabel));
                    OnPropertyChanged(nameof(Direction));
                }
            }
        }

        [CompareAnyValidation]
        public string DestinationCity {
            get { return _destinationCity; }
            set {
                if (_destinationCity != value) {
                    _destinationCity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DestinationLabel));
                    OnPropertyChanged(nameof(Direction));
                }
            }
        }

        public string OriginLabel {
            get {
                if (OriginCity != Parent1?.Load.OriginCity) {
                    return "(stop)";
                }
                return String.Empty;
            }
        }

        public string DestinationLabel {
            get {
                if (DestinationCity != Parent1?.Load.DestinationCity) {
                    return "(stop)";
                }
                return String.Empty;
            }
        }

        public string Direction => $"{OriginCity} → {DestinationCity}";

        public string OriginAddress {
            get { return _originAddress; }
            set {
                if (_originAddress != value) {
                    _originAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DestinationAddress {
            get { return _destinationAddress; }
            set {
                if (_destinationAddress != value) {
                    _destinationAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public LocationViewModel OriginLocation {
            get { return _originLocation; }
            set {
                if (_originLocation != value) {
                    _originLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public LocationViewModel DestinationLocation {
            get { return _destinationLocation; }
            set {
                if (_destinationLocation != value) {
                    _destinationLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Daylight or Standart UTC Offset of Origin Location </summary>
        public int CurrentOriginUtcOffset { get; set; }
        /// <summary>
        ///     Daylight or Standart UTC Offset of Destination Location </summary>
        public int CurrentDestinationUtcOffset { get; set; }

        public int? Distance {
            get { return _distance; }
            set {
                if (_distance != value) {
                    _distance = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? ShipperId { get; set; }

        public string ShipperCompany {
            get { return _shipperCompany; }
            set {
                if (_shipperCompany != value) {
                    _shipperCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PickupNumber {
            get => pickupNumber;
            set {
                if (pickupNumber != value) {
                    pickupNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DeliveryNumber {
            get => deliveryNumber;
            set {
                if (deliveryNumber != value) {
                    deliveryNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? ReceiverId { get; set; }

        public string ReceiverCompany {
            get { return _receiverCompany; }
            set {
                if (_receiverCompany != value) {
                    _receiverCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowPickupAddress { get; set; }

        public AutoCallEquipmentType Equipment {
            get { return _equipment; }
            set {
                if (_equipment != value) {
                    _equipment = value;
                    OnPropertyChanged();
                }
            }
        }

        public PalletType PalletType {
            get { return palletType; }
            set {
                if (palletType != value) {
                    palletType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     For the trailer layout </summary>
        public DivideType DivideType {
            get { return _divideType; }
            set {
                if (_divideType != value) {
                    _divideType = value;
                    OnPropertyChanged();
                }
            }
        }

        public ContainerType ContainerType {
            get { return _containerType; }
            set {
                if (_containerType != value) {
                    _containerType = value;
                    OnPropertyChanged();
                    // TODO: remove if doesn't used
                    OnPropertyChanged("Weight");
                    OnPropertyChanged("Length");
                    OnPropertyChanged("Pallets");
                    OnPropertyChanged("Container");
                }
            }
        }

        public bool Container => ContainerType == ContainerType.Quantity || ContainerType == ContainerType.Distance;
        public bool ContainerAllowAdd => BoardType == DispatchBoardType.Carrier || BoardType == DispatchBoardType.Broker;
        public bool ContainerAllowSplit => BoardType == DispatchBoardType.Carrier;

        public Guid? ParentId {
            get { return _parentId; }
            set {
                if (_parentId != value) {
                    _parentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? ShipmentId {
            get { return _shipmentId; }
            set {
                if (_shipmentId != value) {
                    _shipmentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? HistoryShipmentId {
            get { return _historyShipmentId; }
            set {
                if (_historyShipmentId != value) {
                    _historyShipmentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Assigned {
            get {
                return TrailerId != null || WarehouseId != null || DatDriverId != null || DatCompanyId != null;
            }
        }

        public LoadType LoadType {
            get { return _loadType; }
            set {
                if (_loadType != value) {
                    _loadType = value;
                    OnPropertyChanged();
                }
            }
        }

        [SumValidation(OnlyDispatch = true)]
        public int? Weight {
            get { return _weight; }
            set {
                if (_weight != value) {
                    _weight = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WeightValue));
                    OnPropertyChanged(nameof(WeightString));
                }
            }
        }

        public int? WeightValue => this.WeightType == WeightType.Lbs && this.Weight != null ? this.Weight / 1000 : this.Weight;

        public string WeightString => this.WeightType == WeightType.Lbs ? $"{this.Weight}Lbs" : this.Weight?.ToString();

        [SumValidation(OnlyDispatch = true)]
        public int? Length {
            get { return _length; }
            set {
                if (_length != value) {
                    _length = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     this parent contains subloads for carrier dispatch </summary>
        [NoAssign, NoClone]
        public Wrapper1LoadViewModel Parent1 {
            get { return _parent1; }
            set {
                if (_parent1 != value) {
                    _parent1 = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     this parent contains shipments for broker dispatch </summary>
        [NoAssign, NoClone]
        public Wrapper1LoadViewModel Parent2 {
            get { return _parent2; }
            set {
                if (_parent2 != value) {
                    _parent2 = value;
                    OnPropertyChanged();
                }
            }
        }

        [SumValidation(OnlyDispatch = true), NotNullValidation(OnlyDispatch = true)]
        public short? Pallets {
            get { return _pallets; }
            set {
                if (_pallets != value) {
                    _pallets = value;
                    OnPropertyChanged();
                }
            }
        }

        [SumValidation(OnlyDispatch = true)]
        public short? Spots {
            get { return _spots; }
            set {
                if (_spots != value) {
                    _spots = value;
                    OnPropertyChanged();
                }
            }
        }

        public WeightType WeightType {
            get { return _weightType; }
            set {
                if (_weightType != value) {
                    _weightType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Commodity {
            get { return _commodity; }
            set {
                if (_commodity != value) {
                    _commodity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string GetMeasure() {
            if (Pallets != null) {
                return $"{Pallets} pallets";
            }
            if (Weight != null) {
                return $"{Weight} kLbs";
            }
            if (Length != null) {
                return $"{Length} ft";
            }
            return "-";
        }

        public TrailerType TrailerType {
            get { return _trailerType; }
            set {
                if (_trailerType != value) {
                    _trailerType = value;
                    OnPropertyChanged();
                }
            }
        }

        public short? Temperature {
            get { return _temperature; }
            set {
                if (_temperature != value) {
                    _temperature = value;
                    OnPropertyChanged();
                }
            }
        }

        public short? EquipmentTemperature => Equipment == AutoCallEquipmentType.Reefer ? _temperature : null;

        public DateTime BookedDate {
            get { return _bookedDate; }
            set {
                if (_bookedDate != value) {
                    _bookedDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime BoardChanged {
            get { return _boardChanged; }
            set {
                if (_boardChanged != value) {
                    _boardChanged = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Age));
                }
            }
        }

        public TimeSpan Age => DateTime.Now - BoardChanged;

        public Action LineHaulChanged;

        public decimal? CarrierViewTracking {
            get { return _carrierViewTracking; }
            set {
                if (_carrierViewTracking != value) {
                    _carrierViewTracking = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Rate));
                }
            }
        }

        [SumValidation(OnlyDispatch = true)]
        public decimal? LineHaul {
            get { return _lineHaul; }
            set {
                if (_lineHaul != value) {
                    _lineHaul = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Rate));
                    // to change children
                    LineHaulChanged?.Invoke();
                }
            }
        }

        [SumValidation(OnlyDispatch = true)]
        [NotNullValidation]
        public decimal? Rate {
            get {
                if (LineHaul == null && GainsView.Any(e => e.Cost != null) == false && this.CarrierViewTracking == null) {
                    return null;
                }
                return (LineHaul ?? 0) + GainsView.Sum(e => e.Cost) + (this.CarrierViewTracking ?? 0);
            }
        }

        [DatRateValidation]
        public decimal? DatRate {
            get { return _datRate; }
            set {
                if (_datRate != value) {
                    _datRate = value;
                    OnPropertyChanged();
                }
            }
        }

        // NOTE: don't use dictionary! a order is important!
        [NoAssign, NoClone]
        public List<ExpenseViewModel> Expenses {
            get { return _expenses; }
            set {
                if (_expenses != value) {
                    _expenses = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Rate));
                }
            }
        }

        // NOTE: don't use dictionary! a order is important!
        [NoAssign, NoClone]
        public List<GainViewModel> Gains {
            get { return _gains; }
            set {
                if (_gains != value) {
                    _gains = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Rate));
                }
            }
        }

        #region [ Expenses / Gains ]

        [NoAssign, NoClone]
        public List<ExpenseViewModel> ExpensesView => Parent1 != null ? AutoDispatch.Logic.Helpers.DispatchHelper.Ratio(Parent1.Load, this, Parent1.Load.Expenses) : Expenses;
        [NoAssign, NoClone]
        public List<GainViewModel> GainsView => Parent1 != null ? AutoDispatch.Logic.Helpers.DispatchHelper.Ratio(Parent1.Load, this, Parent1.Load.Gains) : Gains;

        [NoAssign, NoClone]
        public decimal? ExpensesTotal => ExpensesView.Any(e => e.Cost != null) ? ExpensesView.Sum(e => e.Cost) : null;
        [NoAssign, NoClone]
        public decimal? GainsTotal => GainsView.Any(e => e.Cost != null) ? GainsView.Sum(e => e.Cost) : null;
        [NoAssign, NoClone]
        public string ExpensesString => String.Join(", ", ExpensesView.Where(e => e.Cost != null).Select(e => $"{e.Type.GetDescriptionShort()} {Currency.GetDescription()}{e.Cost:N2}"));
        [NoAssign, NoClone]
        public string GainsString => String.Join(", ", GainsView.Where(e => e.Cost != null).Select(e => $"{e.Type.GetDescriptionShort()} {Currency.GetDescription()}{e.Cost:N2}"));
        [NoAssign, NoClone]
        public string ExpensesToolTip => String.Join("\n", ExpensesView.Where(e => e.Cost != null).Select(e => $"{e.Type.GetDescription()}: {Currency.GetDescription()}{e.Cost:N2} {e.Comment}"));
        [NoAssign, NoClone]
        public string GainsToolTip => String.Join("\n", GainsView.Where(e => e.Cost != null).Select(e => $"{e.Type.GetDescription()}: {Currency.GetDescription()}{e.Cost:N2} {e.Comment}"));

        public void ExpensesChanged() {
            OnPropertyChanged(nameof(ExpensesString));
            OnPropertyChanged(nameof(ExpensesToolTip));
            OnPropertyChanged(nameof(ExpensesTotal));
            OnPropertyChanged(nameof(ExpensesView));
            OnPropertyChanged(nameof(Rate));
        }
        public void GainsChanged() {
            OnPropertyChanged(nameof(GainsString));
            OnPropertyChanged(nameof(GainsToolTip));
            OnPropertyChanged(nameof(GainsTotal));
            OnPropertyChanged(nameof(GainsView));
            OnPropertyChanged(nameof(Rate));
        }

        public void ExpenseInvertPaid(ExpenseType type) {
            var expense = this.Expenses.First(e => e.Type == type);
            expense.Paid = !expense.Paid;
        }
        public void GainInvertPaid(GainType type) {
            var gain = this.Gains.First(e => e.Type == type);
            gain.Paid = !gain.Paid;
        }

        #endregion [ Expenses / Gains ]

        public CurrencyType Currency {
            get { return _currency; }
            set {
                if (_currency != value) {
                    _currency = value;
                    OnPropertyChanged();
                }
            }
        }

        [QuikPayValidation]
        public DateTime? QuikPay {
            get { return _quikPay; }
            set {
                if (_quikPay != value) {
                    _quikPay = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime PickupDate {
            get { return _pickupDate; }
            set {
                if (_pickupDate != value) {
                    _pickupDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Pickup");
                }
            }
        }

        public TimeSpan PickupFrom {
            get { return _pickupFrom; }
            set {
                if (_pickupFrom != value) {
                    if (_pickupFrom == _pickupTo) {
                        PickupTo = value;
                    }
                    _pickupFrom = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Pickup");
                }
            }
        }

        public TimeSpan PickupTo {
            get { return _pickupTo; }
            set {
                if (_pickupTo != value) {
                    _pickupTo = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Pickup");
                }
            }
        }

        /// <summary>
        ///     Gets formatted date. Readonly. </summary>
        [PickupValidation]
        [CompareAnyValidation]
        public string Pickup => DispatchHelper.DateTimeToAppointment(PickupDate, PickupFrom, PickupTo);

        /// <summary>
        ///     Gets formatted date. Readonly. </summary>
        public string PickupFullDateTime => DispatchHelper.DateTimeToAppointmentFullDateTime(PickupDate, PickupFrom, PickupTo);

        public DateTime DeliveryDate {
            get { return _deliveryDate; }
            set {
                if (_deliveryDate != value) {
                    _deliveryDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Delivery));
                    OnPropertyChanged(nameof(Late));
                }
            }
        }

        public TimeSpan DeliveryFrom {
            get { return _deliveryFrom; }
            set {
                if (_deliveryFrom != value) {
                    if (_deliveryFrom == _deliveryTo) {
                        DeliveryTo = value;
                    }
                    _deliveryFrom = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Delivery");
                }
            }
        }

        public TimeSpan DeliveryTo {
            get { return _deliveryTo; }
            set {
                if (_deliveryTo != value) {
                    _deliveryTo = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Delivery");
                }
            }
        }

        /// <summary>
        ///     Gets formatted date. Readonly. </summary>
        [DeliveryValidation]
        [CompareAnyValidation]
        public string Delivery => DispatchHelper.DateTimeToAppointment(DeliveryDate, DeliveryFrom, DeliveryTo);

        /// <summary>
        ///     Gets formatted date. Readonly. </summary>
        public string DeliveryFullDateTime => DispatchHelper.DateTimeToAppointmentFullDateTime(DeliveryDate, DeliveryFrom, DeliveryTo);

        #region [ Comments ]

        public string Comments {
            get { return _comments; }
            set {
                if (_comments != value) {
                    _comments = value;
                    if (String.IsNullOrEmpty(value) == false) {
                        try {
                            var doc = XDocument.Parse(value);
                            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                            namespaceManager.AddNamespace("t", "clr-namespace:Telerik.Windows.Documents.Model;assembly=Telerik.Windows.Documents");
                            _comment1Spans = doc.XPathSelectElements(@"t:RadDocument//t:Section//t:Paragraph//t:Span", namespaceManager).ToList();
                        } catch {
                            _comment1Spans = null;
                        }
                    } else {
                        _comment1Spans = null;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Comment1Text));
                    OnPropertyChanged(nameof(Comment1Foreground));
                    OnPropertyChanged(nameof(Comment1Background));
                }
            }
        }

        public string Comments2 {
            get { return _comments2; }
            set {
                if (_comments2 != value) {
                    _comments2 = value;
                    if (String.IsNullOrEmpty(value) == false) {
                        try {
                            var doc = XDocument.Parse(value);
                            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                            namespaceManager.AddNamespace("t", "clr-namespace:Telerik.Windows.Documents.Model;assembly=Telerik.Windows.Documents");
                            _comment2Spans = doc.XPathSelectElements(@"t:RadDocument//t:Section//t:Paragraph//t:Span", namespaceManager).ToList();
                        } catch {
                            _comment2Spans = null;
                        }
                    } else {
                        _comment2Spans = null;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Comment2Text));
                    OnPropertyChanged(nameof(Comment2Foreground));
                    OnPropertyChanged(nameof(Comment2Background));
                }
            }
        }

        public string Consolidation {
            get { return _consolidation; }
            set {
                if (_consolidation != value) {
                    _consolidation = value;
                    if (String.IsNullOrEmpty(value) == false) {
                        try {
                            var doc = XDocument.Parse(value);
                            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                            namespaceManager.AddNamespace("t", "clr-namespace:Telerik.Windows.Documents.Model;assembly=Telerik.Windows.Documents");
                            _consolidationSpans = doc.XPathSelectElements(@"t:RadDocument//t:Section//t:Paragraph//t:Span", namespaceManager).ToList();
                        } catch {
                            _consolidationSpans = null;
                        }
                    } else {
                        _consolidationSpans = null;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConsolidationText));
                    OnPropertyChanged(nameof(ConsolidationForeground));
                    OnPropertyChanged(nameof(ConsolidationBackground));
                }
            }
        }

        // Extra
        private List<XElement> _comment1Spans;
        // Extra
        public string Comment1Text {
            get {
                try {
                    if (_comment1Spans != null && _comment1Spans.Any()) {
                        return String.Join(" ", _comment1Spans.Select(e => e.Attribute("Text").Value)).ReplaceAll("  ", " ");
                    }
                    return String.Empty;
                } catch {
                    return "???";
                }
            }
        }
        // Extra
        public SolidColorBrush Comment1Foreground {
            get {
                if (_comment1Spans != null && _comment1Spans.Any()) {
                    try {
                        var hex = _comment1Spans.First().Attribute("ForeColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Black);
            }
        }
        // Extra
        public SolidColorBrush Comment1Background {
            get {
                if (_comment1Spans != null && _comment1Spans.Any()) {
                    try {
                        var hex = _comment1Spans.First().Attribute("HighlightColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        // Extra
        private List<XElement> _comment2Spans;
        // Extra
        public string Comment2Text {
            get {
                try {
                    if (_comment2Spans != null && _comment2Spans.Any()) {
                        return String.Join(" ", _comment2Spans.Select(e => e.Attribute("Text").Value)).ReplaceAll("  ", " ");
                    }
                    return String.Empty;
                } catch {
                    return "???";
                }
            }
        }
        // Extra
        public SolidColorBrush Comment2Foreground {
            get {
                if (_comment2Spans != null && _comment2Spans.Any()) {
                    try {
                        var hex = _comment2Spans.First().Attribute("ForeColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Black);
            }
        }
        // Extra
        public SolidColorBrush Comment2Background {
            get {
                if (_comment2Spans != null && _comment2Spans.Any()) {
                    try {
                        var hex = _comment2Spans.First().Attribute("HighlightColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }


        // Extra
        private List<XElement> _consolidationSpans;
        // Extra
        public string ConsolidationText {
            get {
                try {
                    if (_consolidationSpans != null && _consolidationSpans.Any()) {
                        return String.Join(" ", _consolidationSpans.Select(e => e.Attribute("Text").Value)).ReplaceAll("  ", " ");
                    }
                    return String.Empty;
                } catch {
                    return "???";
                }
            }
        }
        // Extra
        public SolidColorBrush ConsolidationForeground {
            get {
                if (_consolidationSpans != null && _consolidationSpans.Any()) {
                    try {
                        var hex = _consolidationSpans.First().Attribute("ForeColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Black);
            }
        }
        // Extra
        public SolidColorBrush ConsolidationBackground {
            get {
                if (_consolidationSpans != null && _consolidationSpans.Any()) {
                    try {
                        var hex = _consolidationSpans.First().Attribute("HighlightColor")?.Value;
                        if (hex != null) {
                            return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex));
                        }
                    } catch {
                    }
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        #endregion [ Comments ]

        /// <summary>
        ///     Gets or sets record text color </summary>
        public Color Foreground {
            get { return _foreground; }
            set {
                if (_foreground != value) {
                    _foreground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets record background color </summary>
        public Color Background {
            get { return _background; }
            set {
                if (_background != value) {
                    _background = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? BindDate {
            get { return _bindDate; }
            set {
                if (_bindDate != value) {
                    _bindDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? AssignedTrailerDate {
            get { return _assignedTrailerDate; }
            set {
                if (_assignedTrailerDate != value) {
                    _assignedTrailerDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Index {
            get { return _index; }
            set {
                if (_index != value) {
                    _index = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IndexString));
                }
            }
        }

        public string IndexExtra {
            get { return _indexExtra; }
            set {
                if (_indexExtra != value) {
                    _indexExtra = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IndexString));
                }
            }
        }

        public string IndexPrefix {
            get { return _indexPrefix; }
            set {
                if (_indexPrefix != value) {
                    _indexPrefix = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly int MAX_INT_LENGTH = Int32.MaxValue.ToString().Length;
        // сортировка как по Index цифре, если нет, то по extra
        public string IndexSort {
            get {
                // 1234/2 -> 0000001234/2
                return new string('0', MAX_INT_LENGTH - Index.ToString().Length) + Index.ToString() + IndexExtra;
            }
        }

        [NoAssign, NoClone]
        public string IndexString {
            get { return $"{IndexPrefix}{Index}{IndexExtra}"; }
            set {
                var match = Regex.Match(value.Trim(), @"^(?<prefix>\D*)(?<index>\d{1,8})(?<extra>.*)$");
                if (match.Success) {
                    Index = Int32.Parse(match.Groups["index"].Value);
                    IndexExtra = !String.IsNullOrEmpty(match.Groups["extra"].Value) ? match.Groups["extra"].Value : null;
                    IndexPrefix = match.Groups["prefix"].Value;
                } else {
                    Index = null;
                    IndexExtra = value;
                }
                OnPropertyChanged();
            }
        }

        [NoAssign, NoClone]
        public byte RelativeIndex {
            get { return _relativeIndex; }
            set {
                if (_relativeIndex != value) {
                    _relativeIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoadStatus Status {
            get { return _status; }
            set {
                if (_status != value) {
                    var oldStatus = _status;
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusString));
                    OnPropertyChanged(nameof(Delivery));
                    OnPropertyChanged(nameof(Pickup));
                }
            }
        }

        public Guid? CustomStatusId {
            get { return _customStatusId; }
            set {
                if (_customStatusId != value) {
                    _customStatusId = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastStatusChangedDate { get; set; } = DateTime.UtcNow;

        public sbyte? LastRateChanged {
            get => _lastRateChanged;
            set {
                if (_lastRateChanged != value) {
                    _lastRateChanged = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LastRateChangedString));
                }
            }
        }

        public string LastRateChangedString => LastRateChanged > 0 ? $"+{LastRateChanged}" : LastRateChanged.ToString();

        public string StatusString {
            get {
                if (Status == LoadStatus.Completed) {
                    return $"{Status.GetDescription()} {LastStatusChangedDate:d}";
                } else {
                    return Status.GetDescription();
                }
            }
        }

        public byte? PickupOrder {
            get { return _pickupOrder; }
            set {
                if (_pickupOrder != value) {
                    _pickupOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte? DeliveryOrder {
            get { return _deliveryOrder; }
            set {
                if (_deliveryOrder != value) {
                    _deliveryOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        public ContactsViewModel ShipperContacts {
            get { return _shipperContacts; }
            set {
                if (_shipperContacts != value) {
                    _shipperContacts = value;
                    OnPropertyChanged();
                }
            }
        }

        public ContactsViewModel ReceiverContacts {
            get { return _receiverContacts; }
            set {
                if (_receiverContacts != value) {
                    _receiverContacts = value;
                    OnPropertyChanged();
                }
            }
        }

        public DispatchBoardType BoardType {
            get { return _boardType; }
            set {
                if (_boardType != value) {
                    _boardType = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? BOLDate {
            get { return _bolDate; }
            set {
                if (_bolDate != value) {
                    _bolDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BOLDateView));
                }
            }
        }

        public string BOLFile {
            get { return _bolFile; }
            set {
                if (_bolFile != value) {
                    _bolFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BOLFileView));
                }
            }
        }

        public DateTime? PODDate {
            get { return _podDate; }
            set {
                if (_podDate != value) {
                    _podDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PODDateView));
                }
            }
        }

        public string PODFile {
            get { return _podFile; }
            set {
                if (_podFile != value) {
                    _podFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PODFileView));
                }
            }
        }

        public DateTime? LumperDate {
            get { return _lumperDate; }
            set {
                if (_lumperDate != value) {
                    _lumperDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LumperFile {
            get { return _lumperFile; }
            set {
                if (_lumperFile != value) {
                    _lumperFile = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public DateTime? PaperworkDate {
            get { return _paperworkDate; }
            set {
                if (_paperworkDate != value) {
                    _paperworkDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PaperworkDateView));
                }
            }
        }

        public string PaperworkFile {
            get { return _paperworkFile; }
            set {
                if (_paperworkFile != value) {
                    _paperworkFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PaperworkFileView));
                }
            }
        }

        public DateTime? ConfirmationDate {
            get { return _confirmationDate; }
            set {
                if (_confirmationDate != value) {
                    _confirmationDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConfirmationDateView));
                }
            }
        }

        public string ConfirmationFile {
            get { return _confirmationFile; }
            set {
                if (_confirmationFile != value) {
                    _confirmationFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConfirmationFileView));
                }
            }
        }

        public DateTime? ConfirmedDate {
            get { return _confirmedDate; }
            set {
                if (_confirmedDate != value) {
                    _confirmedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ConfirmedDateView));
                }
            }
        }

        public string ConfirmedFile {
            get { return _confirmedFile; }
            set {
                if (_confirmedFile != value) {
                    _confirmedFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? EmailConfirmationSentDate {
            get { return _emailConfirmationSentDate; }
            set {
                if (_emailConfirmationSentDate != value) {
                    _emailConfirmationSentDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> EmailConfirmationSent {
            get => _emailConfirmationSent;
            set {
                if (_emailConfirmationSent != value) {
                    _emailConfirmationSent = value;
                    OnPropertyChanged();
                }
            }
        }
        [NoAssign, NoClone]
        public Guid? ConfirmationEntityIdView => OldOrderId ?? ParentId ?? EntityId;
        [NoAssign, NoClone, NotNullValidation(NullParent = true, NotSplit = true)]
        public string ConfirmationFileView {
            get { return Parent1?.Load.ConfirmedFile ?? Parent1?.Load.ConfirmationFile ?? ConfirmedFile ?? ConfirmationFile; }
        }
        [NoAssign, NoClone]
        public DateTime? ConfirmationDateView {
            get { return Parent1?.Load.ConfirmationDate ?? ConfirmationDate; }
        }
        [NoAssign, NoClone]
        public DateTime? ConfirmedDateView => Parent1?.Load.ConfirmedDate ?? ConfirmedDate;
        [NoAssign, NoClone]
        public DateTime? EmailConfirmationSentDateView => Parent1?.Load.EmailConfirmationSentDate ?? EmailConfirmationSentDate;
        [NoAssign, NoClone]
        public List<string> EmailConfirmationSentView => Parent1?.Load.EmailConfirmationSent ?? EmailConfirmationSent;

        [NoAssign, NoClone]
        public Guid? EntityIdView => ParentId ?? EntityId;
        [NoAssign, NoClone, NotNullValidation(NullParent = true, OnlyAccounting = true)]
        public string BOLFileView {
            get { return Parent1?.Load.BOLFile ?? BOLFile; }
        }
        [NoAssign, NoClone]
        public DateTime? BOLDateView {
            get { return Parent1?.Load.BOLDate ?? BOLDate; }
        }
        [NoAssign, NoClone, NotNullValidation(NullParent = true, OnlyAccounting = true)]
        public string PODFileView {
            get { return Parent1?.Load.PODFile ?? PODFile; }
        }
        [NoAssign, NoClone]
        public DateTime? PODDateView {
            get { return Parent1?.Load.PODDate ?? PODDate; }
        }
        [NoAssign, NoClone]
        public string InvoiceFileView {
            get { return Parent1?.Load.InvoiceFile ?? InvoiceFile; }
        }
        [NoAssign, NoClone]
        public DateTime? EmailInvoiceSentDateView {
            get { return Parent1?.Load.EmailInvoiceSentDate ?? EmailInvoiceSentDate; }
        }
        [NoAssign, NoClone, NotNullValidation(NullParent = true, OnlyAssigned = true, OnlyDispatch = true)]
        public string PhotoFileView {
            get { return Parent1?.Load.PhotoFile ?? PhotoFile; }
        }
        [NoAssign, NoClone]
        public DateTime? PaperworkDateView {
            get { return Parent1?.Load.PaperworkDate ?? PaperworkDate; }
        }
        [NoAssign, NoClone]
        public string PaperworkFileView {
            get { return Parent1?.Load.PaperworkFile ?? PaperworkFile; }
        }

        [InvoiceValidation]
        public string InvoiceNumber {
            get { return _invoiceNumber; }
            set {
                if (_invoiceNumber != value) {
                    _invoiceNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string InvoiceFile {
            get { return _invoiceFile; }
            set {
                if (_invoiceFile != value) {
                    _invoiceFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InvoiceFileView));
                }
            }
        }

        public DateTime? InvoiceDate {
            get { return _invoiceDate; }
            set {
                if (_invoiceDate != value) {
                    _invoiceDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? TrailerSchemeDate {
            get { return _trailerSchemeDate; }
            set {
                if (_trailerSchemeDate != value) {
                    _trailerSchemeDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TrailerSchemeFile {
            get { return _trailerSchemeFile; }
            set {
                if (_trailerSchemeFile != value) {
                    _trailerSchemeFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? TrailerSchemeId {
            get => trailerSchemeId;
            set {
                if (trailerSchemeId != value) {
                    trailerSchemeId = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? EmailInvoiceSentDate {
            get { return _emailInvoiceSentDate; }
            set {
                if (_emailInvoiceSentDate != value) {
                    _emailInvoiceSentDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> EmailInvoiceSent {
            get => _emailInvoiceSent;
            set {
                if (_emailInvoiceSent != value) {
                    _emailInvoiceSent = value;
                    OnPropertyChanged();
                }
            }
        }

        [CheckValidation]
        public string CheckNumber {
            get { return _checkNumber; }
            set {
                if (_checkNumber != value) {
                    _checkNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CheckFile {
            get { return _checkFile; }
            set {
                if (_checkFile != value) {
                    _checkFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? CheckDate {
            get { return _checkDate; }
            set {
                if (_checkDate != value) {
                    _checkDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public float? Payout {
            get { return _payout; }
            set {
                if (_payout != value) {
                    _payout = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PhotoFile {
            get { return _photoFile; }
            set {
                if (_photoFile != value) {
                    _photoFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PhotoFileView));
                }
            }
        }

        public DateTime? NewScheduledAppointment {
            get { return _newScheduledAppointment; }
            set {
                if (_newScheduledAppointment != value) {
                    _newScheduledAppointment = value;
                    OnPropertyChanged();
                }
            }
        }

        #region [ Company ]

        public int? McNumber {
            get { return _mcNumber; }
            set {
                if (_mcNumber != value) {
                    _mcNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public long? DotNumber {
            get { return _dotNumber; }
            set {
                if (_dotNumber != value) {
                    _dotNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? DatBrokerId { get; set; }
        public Guid? CustomerId { get; set; }

        public string Company {
            get { return _company; }
            set {
                if (_company != value) {
                    _company = value;
                    OnPropertyChanged();
                }
            }
        }

        public ContactsViewModel Contacts {
            get { return _contacts; }
            set {
                if (_contacts != value) {
                    _contacts = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Company ]

        // TODO delete code
        //Status != LoadStatus.Completed &&
        public bool Paid => CheckDate != null && (BOLDate != null || PODDate != null) && InvoiceDate != null && Payout != null && CheckNumber != null;

        /// <summary>
        ///     Gets or sets is record new </summary>
        /// <remarks> Only for view model </remarks>
        [NoAssign, NoClone]
        public bool IsNew {
            get { return _isNew; }
            set {
                _isNew = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     A selection for subloads </summary>
        /// <remarks> Only for view model </remarks>
        [NoAssign, NoClone]
        public bool Mark {
            get { return _mark; }
            set {
                _mark = value;
                OnPropertyChanged();
            }
        }

        public bool WasAtPickup { get; set; }
        public bool WasAtDelivery { get; set; }

        public DateTime? DeliveredDate {
            get { return _deliveredDate; }
            set {
                if (_deliveredDate != value) {
                    _deliveredDate = value;
                    OnPropertyChanged();
                    //Late = value != null && value.Value.Date > DeliveryDate;
                    OnPropertyChanged(nameof(Late));
                }
            }
        }

        [LateValidation]
        public bool Late => DeliveredDate != null && DeliveredDate.Value.Date > DeliveryDate;
        //public bool Late {
        //    get => late;
        //    set {
        //        if (late != value) {
        //            late = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        ///// <summary>
        /////     Gets or sets if record was completed. Load will be crossed </summary>
        ///// <remarks> Only for view model </remarks>
        //[SkipAssign]
        //public bool IsCompleted {
        //    get { return _isCompleted; }
        //    set {
        //        _isCompleted = value;
        //        OnPropertyChanged();
        //    }
        //}

        /// <summary>
        ///     Files </summary>
        public ObservableCollection<FileViewModel> Files { get; set; }

        /// <summary>
        ///     Collection of pallets view models </summary>
        public ObservableCollection<LoadPalletViewModel> Layout { get; set; }

        public bool IsExpanded { get; set; }

        /// <summary>
        ///     Using if Owner edited ActualCompany. Tag: "EditActualCompany" </summary>
        public bool IndexCorrected { get; set; }

        private static Random rand = new Random();
        private sbyte? _lastRateChanged;
        private Guid? trailerSchemeId;
        private string pickupNumber;
        private string deliveryNumber;
        private PalletType palletType;
        private DatDriverViewModel datDriverHistoryObj;
        private int? trackerId;
        private string carrierViewPhone;
        private string carrierViewLink;
        private DateTime? carrierViewLastUpdatedDate;
        private string carrierViewStatus;
        private bool carrierViewLate;
        private int? carrierViewPickupId;
        private int? carrierViewDeliveryId;
        private string carrierViewPickupComment;
        private string carrierViewDeliveryComment;
        private List<string> carrierViewStatuses;

        public LoadViewModel() {
            EntityId = Guid.NewGuid();
            BookedDate = DateTime.Now;
            BoardChanged = DateTime.Now;
            Equipment = AutoCallEquipmentType.Van;
            // NOTE: Includes list of expenses
            Expenses = ExpensesConverter.ToVm(null);
            Gains = GainsConverter.ToVm(null);
            Contacts = new ContactsViewModel();
            ShipperContacts = new ContactsViewModel();
            ReceiverContacts = new ContactsViewModel();
            Files = new ObservableCollection<FileViewModel>();
            Layout = new ObservableCollection<LoadPalletViewModel>();
            PickupDate = DateTime.Now;
            PickupFrom = PickupTo = TimeSpan.FromHours(0);
            DeliveryDate = DateTime.Now;
            DeliveryFrom = DeliveryTo = TimeSpan.FromHours(0);
            ShowPickupAddress = true;
            Foreground = DefaultForeground;
            Background = DefaultBackground;

        }

        public static Color DefaultForeground => Color.FromArgb(255, 0, 0, 0);
        public static Color DefaultBackground {
            get {
                #region Background = random

                // 0 - 256
                var brightnessMin = 0xA0;
                var brightnessMax = 0xF0;
                var color = new int[3];
                var colorK = new double[3] { 0.30, 0.59, 0.11 };

                var brightness = (double)rand.Next(brightnessMin, brightnessMax);

                int rr = 0, gg = 0, bb = 0;
                int k = 0x10;
                while (brightness > 0) {

                    switch (rand.Next(3)) {
                        case 0:
                            if (rr < Byte.MaxValue) {
                                rr += k;
                                brightness -= colorK[0] * k;
                            }
                            break;
                        case 1:
                            if (gg < Byte.MaxValue) {
                                gg += k;
                                brightness -= colorK[1] * k;
                            }
                            break;
                        case 2:
                            if (bb < Byte.MaxValue) {
                                bb += k;
                                brightness -= colorK[2] * k;
                            }
                            break;
                    }
                }

                return Color.FromArgb(255,
                       (byte)(rr < Byte.MaxValue ? rr : Byte.MaxValue),
                       (byte)(gg < Byte.MaxValue ? gg : Byte.MaxValue),
                       (byte)(bb < Byte.MaxValue ? bb : Byte.MaxValue)
                      );

                #endregion Background = random
            }
        }

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return
                  (Status.GetDescriptionOrTostring().ToLower().Contains(search)) ||
                  (IndexString != null && IndexString.ToLower().Contains(search)) ||
                  (Delivery != null && Delivery.ToLower().Contains(search)) ||
                  (Delivery != null && DateTime.TryParse(search, out var dt1) && DeliveryDate.Date == dt1.Date) ||
                  (Pickup != null && Pickup.ToLower().Contains(search)) ||
                  (Pickup != null && DateTime.TryParse(search, out var dt2) && PickupDate.Date == dt2.Date) ||
                  (Length != null && Length.ToString().Contains(search)) ||
                  (Weight != null && Weight.ToString().Contains(search)) ||
                  (Pallets != null && Pallets.ToString().Contains(search)) ||
                  ((Equipment.GetDescription() ?? Equipment.ToString()).Contains(search)) ||
                  (Expenses != null && Expenses.ToString().Contains(search)) ||
                  (Contacts != null && Contacts.ToString().Contains(search)) ||
                  (ShipperContacts != null && ShipperContacts.ToString().Contains(search)) ||
                  (ReceiverContacts != null && ReceiverContacts.ToString().Contains(search)) ||
                  (ShipperCompany != null && ShipperCompany.ToLower().ContainsWithoutSpace(search)) ||
                  (ReceiverCompany != null && ReceiverCompany.ToLower().ContainsWithoutSpace(search)) ||
                  (Company != null && Company.ToLower().ContainsWithoutSpace(search)) ||
                  (Rate != null && Rate.ToString().Contains(search)) ||
                  (LineHaul != null && LineHaul.ToString().Contains(search)) ||
                  (DestinationAddress != null && DestinationAddress.ToLower().Contains(search)) ||
                  (OriginAddress != null && OriginAddress.ToLower().Contains(search)) ||
                  (this.CurrentLocation != null && this.CurrentLocation.FormattedAddress != null && this.CurrentLocation.FormattedAddress.ToLower().Contains(search)) ||
                  (DestinationCity != null && DestinationCity.ToLower().Contains(search)) ||
                  (OriginCity != null && OriginCity.ToLower().Contains(search)) ||
                  (InvoiceNumber != null && InvoiceNumber.ToLower().Contains(search)) ||
                  (CheckNumber != null && CheckNumber.ToLower().Contains(search)) ||
                  (RecordId != null && RecordId.ToLower().Contains(search)) ||
                  (McNumber != null && McNumber.ToString().ToLower().Contains(search)) ||
                  (Int32.TryParse(search.Trim(), out var searchMc) && searchMc == McNumber) ||
                  (FilterHelper.CheckPickup(search, PickupDate)) ||
                  (FilterHelper.CheckDelivery(search, DeliveryDate)) ||
                  (FilterHelper.CheckOrigin(search, OriginCity)) ||
                  (FilterHelper.CheckDestination(search, DestinationCity)) ||
                  (Created.ToLower().Contains(search)) ||
                  (LastModified.ToLower().Contains(search)) ||
                  (Expenses.Any(e => e.Comment != null && e.Comment.ToLower().Contains(search))) ||
                  (Gains.Any(e => e.Comment != null && e.Comment.ToLower().Contains(search))) ||
                  (CarrierViewPhone != null && CarrierViewPhone.ToLower().Contains(search)) ||
                  (CarrierViewStatus != null && CarrierViewStatus.ToLower().Contains(search)) ||
                  (CarrierViewStatuses != null && CarrierViewStatuses.Any(e => e.Contains(search))) ||
                  (carrierViewPickupComment != null && carrierViewPickupComment.ToLower().Contains(search)) ||
                  (carrierViewDeliveryComment != null && carrierViewDeliveryComment.ToLower().Contains(search)) ||
                  (Comment1Text != null && Comment1Text.ToLower().Contains(search)) ||
                  (Comment2Text != null && Comment2Text.ToLower().Contains(search)) ||
                  (ConsolidationText != null && ConsolidationText.ToLower().Contains(search));
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            var result = true;
            foreach (var column in filterColumns.Columns) {
                if (column.Value == null) continue;
                switch (column.Column) {
                    case FilterColumnEnum.Equipment:
                        if (column.Value.Contains("|")) {
                            result = result && column.Value.Split('|').Select(columnValue => columnValue.Trim().ToLower())
                                .Any(columnValue => (Equipment.GetDescription() ?? Equipment.ToString()).ToLower().Contains(columnValue.ToLower()));
                        } else {
                            result = result && (Equipment.GetDescription() ?? Equipment.ToString()).ToLower().Contains(column.Value.ToLower());
                        }
                        break;
                    case FilterColumnEnum.Status:
                        if (column.Value.Contains("|")) {
                            result = result && column.Value.Split('|').Select(columnValue => columnValue.Trim().ToLower())
                                .Any(columnValue => Status.GetDescription().ToLower().Contains(columnValue.ToLower()));
                        } else {
                            result = result && Status.GetDescription().ToLower().Contains(column.Value.ToLower());
                        }
                        break;
                    case FilterColumnEnum.CustomStatus:
                        var sessionCustomStatuses = UserSession.Current.CustomStatuses;
                        if (sessionCustomStatuses != null) {
                            var customStatusNames = column.Value.Split('|').Select(columnValue => columnValue.Trim().ToLower()).ToList();
                            var customStatusIds = sessionCustomStatuses
                                .Where(e => customStatusNames.Contains(e.Value.ToLower())).Select(e => e.Key).ToList();
                            if (column.Value.Contains("|")) {
                                result = result && customStatusIds.Any(e => CustomStatusId == e);
                            } else {
                                result = result && CustomStatusId == customStatusIds.FirstOrDefault();
                            }
                        }
                        break;
                    case FilterColumnEnum.Origin:
                        if (column.Value.All(e => Char.IsUpper(e) || Char.IsDigit(e)) == false) {
                            result = result && OriginCity.ToLower().Contains(column.Value.ToLower());
                        } else {
                            result = result && FilterHelper.CheckStates(OriginCity, column.Value);
                        }
                        break;
                    case FilterColumnEnum.Destination:
                        if (column.Value.All(e => Char.IsUpper(e) || Char.IsDigit(e)) == false) {
                            result = result && DestinationCity.ToLower().Contains(column.Value.ToLower());
                        } else {
                            result = result && FilterHelper.CheckStates(DestinationCity, column.Value);
                        }
                        break;
                    case FilterColumnEnum.Pickup: {
                            if (Enum.TryParse<DayOfWeek>(column.Value.FirstCharToUpper(), out var dayOfWeek)) {
                                result = PickupDate.DayOfWeek == dayOfWeek;
                            }
                            {
                                var match = Regex.Match(column.Value, FilterHelper.DateSpanRegex);
                                if (match.Success) {
                                    result = result && FilterHelper.CheckDateSpan(match, PickupDate, column.Value);
                                }
                            }
                            {
                                var match = Regex.Match(column.Value, FilterHelper.DateSpanFromRegex);
                                if (match.Success) {
                                    result = result && FilterHelper.CheckDateFromSpan(match, PickupDate, column.Value);
                                }
                            }
                            {
                                var match = Regex.Match(column.Value, FilterHelper.DateSpanToRegex);
                                if (match.Success) {
                                    result = result && FilterHelper.CheckDateToSpan(match, PickupDate, column.Value);
                                }
                            }
                            {
                                var match = Regex.Match(column.Value, FilterHelper.DateSpanBetweenRegex);
                                if (match.Success) {
                                    result = result && FilterHelper.CheckDateBetweenSpan(match, PickupDate, column.Value);
                                }
                            }
                        }
                        break;
                    case FilterColumnEnum.Delivery: {
                            var func = new Func<DateTime, bool>(date => {
                                bool r = true;
                                if (Enum.TryParse<DayOfWeek>(column.Value.FirstCharToUpper(), out var dayOfWeek)) {
                                    r = date.DayOfWeek == dayOfWeek;
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanFromRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateFromSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanToRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateToSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanBetweenRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateBetweenSpan(match, date, column.Value);
                                    }
                                }
                                return r;
                            });
                            result = result && (
                                func(this.DeliveryDate) || (this.NewScheduledAppointment != null) && func(this.NewScheduledAppointment.Value));
                        }
                        break;
                    case FilterColumnEnum.Created: {
                            var func = new Func<DateTime, bool>(date => {
                                bool r = true;
                                if (Enum.TryParse<DayOfWeek>(column.Value.FirstCharToUpper(), out var dayOfWeek)) {
                                    r = date.DayOfWeek == dayOfWeek;
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanFromRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateFromSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanToRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateToSpan(match, date, column.Value);
                                    }
                                }
                                {
                                    var match = Regex.Match(column.Value, FilterHelper.DateSpanBetweenRegex);
                                    if (match.Success) {
                                        r = r && FilterHelper.CheckDateBetweenSpan(match, date, column.Value);
                                    }
                                }
                                return r;
                            });
                            result = result && func(this.CreatedDate);
                        }
                        break;
                    case FilterColumnEnum.CreatedByUser:
                        result = result && CreatedByName?.ToLower() == column.Value.ToLower();
                        break;
                    case FilterColumnEnum.Index:
                        result = result && this.IndexString.Contains(column.Value);
                        break;
                }
            }
            return result;
        }

        public override object Clone() {
            var clone = (LoadViewModel)base.Clone();

            clone.Files = new ObservableCollection<FileViewModel>();

            clone.Layout = new ObservableCollection<LoadPalletViewModel>();
            foreach (var pallet in this.Layout) {
                clone.Layout.Add((LoadPalletViewModel)pallet.Clone());
            }

            if (this.Expenses != null) {
                foreach (var expense in this.Expenses) {
                    if (clone.Expenses.Any(e => e.Type == expense.Type)) {
                        clone.Expenses.First(e => e.Type == expense.Type).Assign(expense);
                    } else {
                        clone.Expenses.Add((ExpenseViewModel)expense.Clone());
                    }
                }
            }

            if (this.Expenses != null) {
                foreach (var gain in this.Gains) {
                    if (clone.Gains.Any(e => e.Type == gain.Type)) {
                        clone.Gains.First(e => e.Type == gain.Type).Assign(gain);
                    } else {
                        clone.Gains.Add((GainViewModel)gain.Clone());
                    }
                }
            }

            return clone;
        }

        public override void Assign(object source) {
            var newLoad = (LoadViewModel)source;
            var oldConfirmedDate = this.ConfirmedDate;

            base.Assign(source);
            this.Files = new ObservableCollection<FileViewModel>();

            this.Layout = new ObservableCollection<LoadPalletViewModel>();
            foreach (var pallet in ((LoadViewModel)source).Layout) {
                this.Layout.Add((LoadPalletViewModel)pallet.Clone());
            }

            if (this.Expenses != null && newLoad.Expenses != null) {
                foreach (var current in this.Expenses) {
                    foreach (var @new in newLoad.Expenses) {
                        if (current.Type == @new.Type) {
                            current.Assign(@new);
                            break;
                        }
                    }
                }
            }

            if (this.Gains != null && newLoad.Gains != null) {
                foreach (var current in this.Gains) {
                    foreach (var @new in newLoad.Gains) {
                        if (current.Type == @new.Type) {
                            current.Assign(@new);
                            break;
                        }
                    }
                }
            }

            // NOTE: if carrier signed a confirmation
            if (oldConfirmedDate == null && this.ConfirmedDate != null) {
                IsNew = true;
            }

            this.Saving = false;
        }

        public void AssignCompany(LoadViewModel load) {
            this.Contacts = load.Contacts;
            this.McNumber = load.McNumber;
            this.Company = load.Company;
            this.DatBrokerId = load.DatBrokerId;
            this.CustomerId = load.CustomerId;
        }

        public void AssignCompany(DatCompanyViewModel company) {
            this.Contacts = company.Contacts;
            this.McNumber = company.McNumber;
            this.Company = company.Name;
        }

        public void SaveHistory(DatCompanyViewModel datCompany) {
            HistoryDatCompanyId = DatCompanyId;
            DatCompanyId = null;
        }

        /// <summary>
        ///     Sets HistoryAssignedIds and Removes AssignedIds  </summary>
        public void SaveHistory(
            DriverViewModel driver, TruckViewModel truck, TrailerViewModel trailer, LoadViewModel shipment, WarehouseViewModel warehouse) {
            if (driver != null) {
                HistoryDriverId = driver.EntityId;
            }
            if (truck != null) {
                HistoryTruckId = truck.EntityId;
            }
            if (trailer != null) {
                HistoryTrailerId = trailer.EntityId;
            }
            if (shipment != null) {
                HistoryShipmentId = shipment.EntityId;
            }
            if (warehouse != null) {
                HistoryWarehouseId = warehouse.EntityId;
            }

            TrailerId = null;
            ShipmentId = null;
            WarehouseId = null;
        }

        public void RestoryHistory() {
            if (HistoryDatCompanyId != null) {
                DatCompanyId = HistoryDatCompanyId;
            }
            if (HistoryDatDriverId != null) {
                DatDriverId = HistoryDatDriverId;
            }
            if (HistoryTrailerId != null) {
                TrailerId = HistoryTrailerId;
            }
            if (HistoryShipmentId != null) {
                ShipmentId = HistoryShipmentId;
            }
            if (HistoryWarehouseId != null) {
                WarehouseId = HistoryWarehouseId;
            }
            HistoryDatCompanyId = null;
            HistoryDatDriverId = null;
            HistoryDriverId = null;
            HistoryTruckId = null;
            HistoryTrailerId = null;
            HistoryShipmentId = null;
            HistoryWarehouseId = null;
        }

        public override string ToString() {
            return $"{this.IndexString}";
        }
    }
}
