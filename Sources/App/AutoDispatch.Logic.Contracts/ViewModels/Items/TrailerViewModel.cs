using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoCall.Common;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class TrailerViewModel : BaseDispatchViewModel {

        private string _number;
        private TrailerType _type;
        private Guid? _yardId;
        private double _height;
        private double _width;
        private string _annualInspection_File;
        private DateTime? _annualInspection_Expiration;
        private string _registration_File;
        private string _maintenance_File;
        private DateTime? _maintenance_Expiration;

        private string _bnsfOwner;
        private string _bnsfShipper;
        private string _bnsfUnit;
        private string _bnsfLoadEmptyStatus;
        private string _bnsfCurrentLocation;
        private string _bnsfEventCode;
        private string _bnsfEventDate;
        private string _bnsfETA_Deramp;
        private string _bnsfOrigin;
        private string _bnsfDestination;
        private string _bnsfBeyondGoalHours;
        private string _bnsfTimeDeramp;
        private string _bnsfFinalTrainArrival;
        private string _bnsfGoalDate;
        private string _bnsfNextevent;
        private string _bnsfTimeNotify;
        private string _bnsfPickUpNumber;
        private string _bnsfStorageDay;
        private string _bnsfTrain;
        private string _bnsfTrucker;
        private string _bnsfWaybillDate;
        private string _bnsfRateAuthority;
        private decimal? _bnsfAmountDue;
        private string _bnsfDueDate;
        private string _bnsfWaybillNumber;
        private string _bnsfBillNumber;
        private string _bnsfAdjustments;
        private string _yard;
        private bool _history;
        private string _SchemeFile;
        private DateTime? _SchemeDate;

        public string Number {
            get { return _number; }
            set {
                if (_number != value) {
                    _number = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerType Type {
            get { return _type; }
            set {
                if (_type != value) {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? YardId {
            get => _yardId;
            set {
                if (_yardId != value) {
                    _yardId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool History {
            get => _history;
            set {
                if (_history != value) {
                    _history = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TrailerCellViewModel> Layout { get; private set; }

        public const int MaxRows = 15;
        public const int MaxColumns = 2;
        /// <summary>
        ///     Max stack of pallets  </summary>
        public const int MaxStack = 4;

        public double Width {
            get { return _width; }
            set {
                if (_width != value) {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Height {
            get { return _height; }
            set {
                if (_height != value) {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        #region [ Files ]

        public string AnnualInspection_File {
            get => _annualInspection_File;
            set {
                if (_annualInspection_File != value) {
                    _annualInspection_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? AnnualInspection_Expiration {
            get => _annualInspection_Expiration;
            set {
                if (_annualInspection_Expiration != value) {
                    _annualInspection_Expiration = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Registration_File {
            get => _registration_File;
            set {
                if (_registration_File != value) {
                    _registration_File = value;
                    OnPropertyChanged();
                }
            }
        }

        //public DateTime? Registration_Expiration { get; set; } // NOTES: without expiration

        public string Maintenance_File {
            get => _maintenance_File;
            set {
                if (_maintenance_File != value) {
                    _maintenance_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? Maintenance_Expiration {
            get => _maintenance_Expiration;
            set {
                if (_maintenance_Expiration != value) {
                    _maintenance_Expiration = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SchemeFile {
            get => _SchemeFile;
            set {
                if (_SchemeFile != value) {
                    _SchemeFile = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? SchemeDate {
            get => _SchemeDate;
            set {
                if (_SchemeDate != value) {
                    _SchemeDate = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Files </summary>
        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();

        #endregion [ Files ]

        public ObservableCollection<DefectViewModel> Defects { get; set; } = new ObservableCollection<DefectViewModel>();

        public string DefectsString => String.Join(", ", Defects.Where(e => e.ServiceNeeded && e.Repair == null).Select(e => ((DefectTrailerEnum)e.DefectId).GetDescription()));

        public bool DefectsServiceNeeded => Defects.Any(e => e.ServiceNeeded && e.Repair == null);

        #region [ BNSF ]

        public string BnsfOwner {
            get => _bnsfOwner;
            set {
                if (_bnsfOwner != value) {
                    _bnsfOwner = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BnsfShipper {
            get => _bnsfShipper;
            set {
                if (_bnsfShipper != value) {
                    _bnsfShipper = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfUnit {
            get => _bnsfUnit;
            set {
                if (_bnsfUnit != value) {
                    _bnsfUnit = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfLoadEmptyStatus {
            get => _bnsfLoadEmptyStatus;
            set {
                if (_bnsfLoadEmptyStatus != value) {
                    _bnsfLoadEmptyStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfCurrentLocation {
            get => _bnsfCurrentLocation;
            set {
                if (_bnsfCurrentLocation != value) {
                    _bnsfCurrentLocation = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfEventCode {
            get => _bnsfEventCode;
            set {
                if (_bnsfEventCode != value) {
                    _bnsfEventCode = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfEventDate {
            get => _bnsfEventDate;
            set {
                if (_bnsfEventDate != value) {
                    _bnsfEventDate = value;
                    OnPropertyChanged();
                }
            }
        }

        [ETA_DerampValidation]
        public string BnsfETA_Deramp {
            get => _bnsfETA_Deramp;
            set {
                if (_bnsfETA_Deramp != value) {
                    _bnsfETA_Deramp = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfOrigin {
            get => _bnsfOrigin;
            set {
                if (_bnsfOrigin != value) {
                    _bnsfOrigin = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfDestination {
            get => _bnsfDestination;
            set {
                if (_bnsfDestination != value) {
                    _bnsfDestination = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfBeyondGoalHours {
            get => _bnsfBeyondGoalHours;
            set {
                if (_bnsfBeyondGoalHours != value) {
                    _bnsfBeyondGoalHours = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfTimeDeramp {
            get => _bnsfTimeDeramp;
            set {
                if (_bnsfTimeDeramp != value) {
                    _bnsfTimeDeramp = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfTimeNotify {
            get => _bnsfTimeNotify;
            set {
                if (_bnsfTimeNotify != value) {
                    _bnsfTimeNotify = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfFinalTrainArrival {
            get => _bnsfFinalTrainArrival;
            set {
                if (_bnsfFinalTrainArrival != value) {
                    _bnsfFinalTrainArrival = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfGoalDate {
            get => _bnsfGoalDate;
            set {
                if (_bnsfGoalDate != value) {
                    _bnsfGoalDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfNextevent {
            get => _bnsfNextevent;
            set {
                if (_bnsfNextevent != value) {
                    _bnsfNextevent = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfPickUpNumber {
            get => _bnsfPickUpNumber;
            set {
                if (_bnsfPickUpNumber != value) {
                    _bnsfPickUpNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfStorageDay {
            get => _bnsfStorageDay;
            set {
                if (_bnsfStorageDay != value) {
                    _bnsfStorageDay = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfTrain {
            get => _bnsfTrain;
            set {
                if (_bnsfTrain != value) {
                    _bnsfTrain = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfTrucker {
            get => _bnsfTrucker;
            set {
                if (_bnsfTrucker != value) {
                    _bnsfTrucker = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfWaybillDate {
            get => _bnsfWaybillDate;
            set {
                if (_bnsfWaybillDate != value) {
                    _bnsfWaybillDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfRateAuthority {
            get => _bnsfRateAuthority;
            set {
                if (_bnsfRateAuthority != value) {
                    _bnsfRateAuthority = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal? BnsfAmountDue {
            get => _bnsfAmountDue;
            set {
                if (_bnsfAmountDue != value) {
                    _bnsfAmountDue = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfDueDate {
            get => _bnsfDueDate;
            set {
                if (_bnsfDueDate != value) {
                    _bnsfDueDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfWaybillNumber {
            get => _bnsfWaybillNumber;
            set {
                if (_bnsfWaybillNumber != value) {
                    _bnsfWaybillNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfBillNumber {
            get => _bnsfBillNumber;
            set {
                if (_bnsfBillNumber != value) {
                    _bnsfBillNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BnsfAdjustments {
            get => _bnsfAdjustments;
            set {
                if (_bnsfAdjustments != value) {
                    _bnsfAdjustments = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Extra </summary>
        public string YardName {
            get => _yard;
            set {
                if (_yard != value) {
                    _yard = value;
                    OnPropertyChanged();
                }
            }
        }


        #endregion [ BNSF ]

        public TrailerViewModel() {
            Layout = new ObservableCollection<TrailerCellViewModel>();
        }

        public override object Clone() {
            var clone = (TrailerViewModel)base.Clone();
            clone.Layout = new ObservableCollection<TrailerCellViewModel>();
            foreach (var trailerCell in Layout) {
                clone.Layout.Add((TrailerCellViewModel)trailerCell.Clone());
            }
            clone.Defects = new ObservableCollection<DefectViewModel>(this.Defects.Select(e => (DefectViewModel)e.Clone()));
            return clone;
        }

        public override void Assign(object source) {
            base.Assign(source);

            this.Layout = new ObservableCollection<TrailerCellViewModel>();
            foreach (var trailerCell in ((TrailerViewModel)source).Layout) {
                this.Layout.Add((TrailerCellViewModel)trailerCell.Clone());
            }
        }

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (
                  (this.CurrentLocation != null && this.CurrentLocation.FormattedAddress != null && this.CurrentLocation.FormattedAddress.ToLower().Contains(search)) ||
                  (this.BnsfOrigin != null && this.BnsfOrigin.ToLower().Contains(search)) ||
                  (this.BnsfDestination != null && this.BnsfDestination.ToLower().Contains(search)) ||
                  (this.BnsfCurrentLocation != null && this.BnsfCurrentLocation.ToLower().Contains(search)) ||
                  (this.BnsfETA_Deramp != null && this.BnsfETA_Deramp.ToLower().Contains(search)) ||
                  (Number != null && Number.ToLower().Contains(search)));
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            foreach (var column in filterColumns.Columns) {
                switch (column.Column) {
                    case FilterColumnEnum.Trailer:
                        result = result && Number.ToLower().Contains(column.Value.ToLower());
                        break;
                }
            }
            return result;
        }

        public void GeneratePallets(IList<LoadViewModel> loads) {
            this.Layout.Clear();
            for (int i = 0; i < MaxRows * MaxColumns; i++) {
                Layout.Add(new TrailerCellViewModel(i));
            }

            foreach (var load in loads) {
                if (load.TrailerId != this.EntityId) continue;
                // Remove old pallets
                foreach (var cell in this.Layout) {
                    foreach (var pallet in cell.Pallets.ToList()) {
                        if (pallet.Load.EntityId == load.EntityId) {
                            cell.Pallets.Remove(pallet);
                        }
                    }
                }
                // Skips if a broker board
                if (load.BoardType != DispatchBoardType.Carrier) {
                    continue;
                }
                foreach (LoadPalletViewModel pallet in load.Layout) {
                    // the cell of trailer
                    var cell = this.Layout.FirstOrDefault(c => c.Index == pallet.Index);
                    if (cell != null) {
                        // loop by pallets
                        for (int i = cell.Pallets.Count - 1; i >= -1; i--) {
                            // Inserts as first or after i
                            if (i == -1 || pallet.Level > cell.Pallets[i].Level) {
                                var existsLoad = loads.FirstOrDefault(l => l.EntityId == load.EntityId);
                                if (existsLoad != null) {
                                    pallet.Load = existsLoad;
                                } else {
                                    _webl.e($"Load was not found. \nLoadId: {load.EntityId}");
                                }
                                cell.Pallets.Insert(i + 1, pallet);
                                break;
                            }
                            // Duplicate
                            if (pallet.Level == cell.Pallets[i].Level) {
                                //_webl.e($"Duplicate level\nTrailerId: {load.TrailerId}\nLoadId: {load.EntityId}\nCell Level: {pallet.Level}");
                                break;
                            }
                        }
                    } else {
                        _webl.e($"Cell was not found\nTrailerId: {load.TrailerId}\nLoadId: {load.EntityId}\nCell Level: {pallet.Level}");
                    }
                }
            }
        }
    }
}
