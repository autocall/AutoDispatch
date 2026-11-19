using System;
using System.Collections.ObjectModel;
using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels.Items {
    /// <summary>
    ///     Driver View Model </summary>
    public class DriverViewModel : BaseDispatchViewModel {
        private string _name;
        private string _email;
        private string _phone;
        private ChatStatus _chatStatus;
        private Guid? _truckId;
        private Guid? _loadId;
        private DriverStatus _driverStatus;
        private string _rANDOM_DRUG_TEST_File;
        private string _dRUG_TEST_File;
        private string _bACKGROUND_File;
        private string _dOT_INSPECTIONS_File;
        private string _cDL_File;
        private string _mEDICAL_CARD_File;
        private string _mVR_File;
        private string _tHE_APPLICATION_OF_EMPLOYMENT_File;
        private string _ACH_File;
        private string _W9_File;

        private int? _carrierViewId;
        private DateTime? _carrierViewLastUpdatedDate;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                }
            }
        }

        public string Email {
            get { return _email; }
            set {
                if (_email != value) {
                    _email = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                }
            }
        }

        public string Phone {
            get { return _phone; }
            set {
                if (_phone != value) {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChatStatus ChatStatus {
            get { return _chatStatus; }
            set {
                if (_chatStatus != value) {
                    _chatStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? TruckId {
            get { return _truckId; }
            set {
                if (_truckId != value) {
                    _truckId = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? LoadId {
            get { return _loadId; }
            set {
                if (_loadId != value) {
                    _loadId = value;
                    OnPropertyChanged();
                }
            }
        }

        public DriverStatus DriverStatus {
            get { return _driverStatus; }
            set {
                if (_driverStatus != value) {
                    _driverStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayName {
            get {
                if (String.IsNullOrEmpty(Name)) {
                    return $"{Email}";
                } else {
                    return $"{Name}";
                }
            }
        }

        #region [ CarrierView Tracker ]

        public int? CarrierViewId {
            get { return _carrierViewId; }
            set {
                if (_carrierViewId != value) {
                    _carrierViewId = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? CarrierViewLastUpdatedDate {
            get { return _carrierViewLastUpdatedDate; }
            set {
                if (_carrierViewLastUpdatedDate != value) {
                    _carrierViewLastUpdatedDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CarrierViewLastUpdated");
                    OnPropertyChanged("CarrierViewLastUpdatedLate");
                }
            }
        }
        public string CarrierViewLastUpdated {
            get {
                if (CarrierViewLastUpdatedDate != null) {
                    return StringExtension.ToString(DateTime.UtcNow - CarrierViewLastUpdatedDate.Value);
                } else {
                    return "-";
                }
            }
        }
        public bool CarrierViewLastUpdatedLate {
            get {
                if (CarrierViewLastUpdatedDate != null) {
                    return DateTime.UtcNow - CarrierViewLastUpdatedDate.Value > TimeSpan.FromHours(1);
                } else {
                    return true;
                }
            }
        }

        #endregion [ CarrierView Tracker ]

        public string DisplayNameAndPhone => $"{this.Phone} ({this.DisplayName})";

        #region [ Files ]

        public string THE_APPLICATION_OF_EMPLOYMENT_File {
            get => _tHE_APPLICATION_OF_EMPLOYMENT_File;
            set {
                if (_tHE_APPLICATION_OF_EMPLOYMENT_File != value) {
                    _tHE_APPLICATION_OF_EMPLOYMENT_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MVR_File {
            get => _mVR_File;
            set {
                if (_mVR_File != value) {
                    _mVR_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MEDICAL_CARD_File {
            get => _mEDICAL_CARD_File;
            set {
                if (_mEDICAL_CARD_File != value) {
                    _mEDICAL_CARD_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CDL_File {
            get => _cDL_File;
            set {
                if (_cDL_File != value) {
                    _cDL_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DOT_INSPECTIONS_File {
            get => _dOT_INSPECTIONS_File;
            set {
                if (_dOT_INSPECTIONS_File != value) {
                    _dOT_INSPECTIONS_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string BACKGROUND_File {
            get => _bACKGROUND_File;
            set {
                if (_bACKGROUND_File != value) {
                    _bACKGROUND_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DRUG_TEST_File {
            get => _dRUG_TEST_File;
            set {
                if (_dRUG_TEST_File != value) {
                    _dRUG_TEST_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RANDOM_DRUG_TEST_File {
            get => _rANDOM_DRUG_TEST_File;
            set {
                if (_rANDOM_DRUG_TEST_File != value) {
                    _rANDOM_DRUG_TEST_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ACH_File {
            get => _ACH_File;
            set {
                if (_ACH_File != value) {
                    _ACH_File = value;
                    OnPropertyChanged();
                }
            }
        }
        public string W9_File {
            get => _W9_File;
            set {
                if (_W9_File != value) {
                    _W9_File = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? MVR_Expiration { get; set; }
        public DateTime? MEDICAL_CARD_Expiration { get; set; }
        public DateTime? CDL_Expiration { get; set; }
        public DateTime? ACH_Expiration { get; set; }
        public DateTime? W9_Expiration { get; set; }

        /// <summary>
        ///     Files </summary>
        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();

        #endregion [ Files ]

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (
                  (Name != null && Name.ToLower().Contains(search)) ||
                  (Email != null && Email.ToLower().Contains(search)));
        }


        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            foreach (var column in filterColumns.Columns) {
                switch (column.Column) {
                    case FilterColumnEnum.Truck:
                        result = result && Name.ToLower().Contains(column.Value.ToLower());
                        break;
                }
            }
            return result;
        }

        public override string ToString() {
            return StringExtension.ObjectToString(this);
        }
    }
}
