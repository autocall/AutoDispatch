using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class DatDriverViewModel : BaseDispatchViewModel {
        private string _name;
        private string _truckNumber;
        private string _trailerNumber;
        private string _contactPhone;
        private Guid _datCompanyId;
        private string _comments;

        private bool _carrierViewLate;
        private string _carrierViewStatus;
        private int? _carrierViewId;
        private string _carrierViewLoadId;
        private string _carrierViewLink;
        private DateTime? _carrierViewLastUpdatedDate;
        private string _carrierViewPickupComment;
        private string _carrierViewDeliveryComment;
        public bool _carrierViewEdit;
        private List<string> carrierViewStatuses;

        public ObservableCollection<Wrapper1LoadViewModel> Loads { get; set; } = new ObservableCollection<Wrapper1LoadViewModel>();

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TruckNumber {
            get { return _truckNumber; }
            set {
                if (_truckNumber != value) {
                    _truckNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TrailerNumber {
            get { return _trailerNumber; }
            set {
                if (_trailerNumber != value) {
                    _trailerNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ContactPhone {
            get { return _contactPhone; }
            set {
                if (_contactPhone != value) {
                    _contactPhone = value;
                    OnPropertyChanged();
                }
            }
        }

        private const string PhoneMask11 = "+# (###) ###-####";
        private const string PhoneMask10 = "(###) ###-####";

        public string ContactPhoneToString {
            get {
                if (this.ContactPhone != null) {
                    if (this.ContactPhone.Length <= 10) {
                        return ContactPhone.ToMask(PhoneMask10);
                    } else {
                        return ContactPhone.ToMask(PhoneMask11);
                    }
                } else {
                    return String.Empty;
                }
            }
        }

        public string Comments {
            get { return _comments; }
            set {
                if (_comments != value) {
                    _comments = value;
                    OnPropertyChanged();
                }
            }
        }

        #region [ CarrierView Tracker ]

        public bool CarrierViewLate {
            get { return _carrierViewLate; }
            set {
                if (_carrierViewLate != value) {
                    _carrierViewLate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewStatus {
            get { return _carrierViewStatus; }
            set {
                if (_carrierViewStatus != value) {
                    _carrierViewStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> CarrierViewStatuses {
            get { return carrierViewStatuses; }
            set {
                if (carrierViewStatuses != value) {
                    carrierViewStatuses = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CarrierViewStatuses1));
                    OnPropertyChanged(nameof(CarrierViewStatuses2));
                    OnPropertyChanged(nameof(CarrierViewStatusesString));
                    OnPropertyChanged(nameof(CarrierViewStatuses1String));
                    OnPropertyChanged(nameof(CarrierViewStatuses2String));
                }
            }
        }
        public List<string> CarrierViewStatuses1 => CarrierViewStatuses?.Where(e => e.StartsWith("tracking_") == true).ToList();
        public List<string> CarrierViewStatuses2 => CarrierViewStatuses?.Where(e => e.StartsWith("tracking_") == false).ToList();
        public string CarrierViewStatusesString => CarrierViewStatuses != null ? String.Join(", ", CarrierViewStatuses) : null;
        public string CarrierViewStatuses1String => CarrierViewStatuses1 != null ? String.Join(", ", CarrierViewStatuses1) : null;
        public string CarrierViewStatuses2String => CarrierViewStatuses2 != null ? String.Join(", ", CarrierViewStatuses2) : null;

        public int? CarrierViewId {
            get { return _carrierViewId; }
            set {
                if (_carrierViewId != value) {
                    _carrierViewId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewLoadId {
            get { return _carrierViewLoadId; }
            set {
                if (_carrierViewLoadId != value) {
                    _carrierViewLoadId = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewLink {
            get { return _carrierViewLink; }
            set {
                if (_carrierViewLink != value) {
                    _carrierViewLink = value;
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
        public string CarrierViewPickupComment {
            get { return _carrierViewPickupComment; }
            set {
                if (_carrierViewPickupComment != value) {
                    _carrierViewPickupComment = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CarrierViewDeliveryComment {
            get { return _carrierViewDeliveryComment; }
            set {
                if (_carrierViewDeliveryComment != value) {
                    _carrierViewDeliveryComment = value;
                    OnPropertyChanged();
                }
            }
        }
        public int? CarrierViewPickupId { get; set; }
        public int? CarrierViewDeliveryId { get; set; }
        public bool CarrierViewEdit {
            get { return _carrierViewEdit; }
            set {
                if (_carrierViewEdit != value) {
                    _carrierViewEdit = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ CarrierView Tracker ]

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (
                  (Name != null && Name.ToLower().Contains(search)) ||
                  (ContactPhone != null && ContactPhone.ToLower().Contains(search)) ||
                  (TrailerNumber != null && TrailerNumber.ToLower().Contains(search)) ||
                  (TruckNumber != null && TruckNumber.ToLower().Contains(search)));
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            return result;
        }

        public void Clear() {
            Loads.Clear();
        }
    }
}
