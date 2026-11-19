using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class TruckViewModel : BaseDispatchViewModel {
        private string _number;
        private Guid? _trailerId;
        private string _annualInspection_File;
        private DateTime? _annualInspection_Expiration;
        private string _registration_File;
        private DateTime? _registration_Expiration;

        public string Number {
            get { return _number; }
            set {
                if (_number != value) {
                    _number = value;
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

        public override bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (
                  (this.CurrentLocation != null && this.CurrentLocation.FormattedAddress != null && this.CurrentLocation.FormattedAddress.ToLower().Contains(search)) ||
                  (Number != null && Number.ToLower().Contains(search)));
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
        public DateTime? Registration_Expiration {
            get => _registration_Expiration;
            set {
                if (_registration_Expiration != value) {
                    _registration_Expiration = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Files </summary>
        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();

        #endregion [ Files ]

        public ObservableCollection<DefectViewModel> Defects { get; set; } = new ObservableCollection<DefectViewModel>();

        public string DefectsString => String.Join(", ", Defects.Where(e => e.ServiceNeeded && e.Repair == null).Select(e => ((DefectTruckEnum)e.DefectId).GetDescription()));

        public bool DefectsServiceNeeded => Defects.Any(e => e.ServiceNeeded && e.Repair == null);

        public override object Clone() {
            var clone = (TruckViewModel)base.Clone();
            clone.Defects = new ObservableCollection<DefectViewModel>(this.Defects.Select(e => (DefectViewModel)e.Clone()));
            return clone;
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            foreach (var column in filterColumns.Columns) {
                switch (column.Column) {
                    case FilterColumnEnum.Truck:
                        result = result && Number.ToLower().Contains(column.Value.ToLower());
                        break;
                }
            }
            return result;
        }
    }
}
