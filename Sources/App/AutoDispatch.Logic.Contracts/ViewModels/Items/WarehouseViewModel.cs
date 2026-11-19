using System;
using AutoDispatch.Logic.Contracts.Enums;
using System.Linq;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class WarehouseViewModel : BaseObjectViewModel {

        private string _name;
        private string _city;
        private string _address;
        private LocationViewModel _location;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string City {
            get { return _city; }
            set {
                if (_city != value) {
                    _city = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Address {
            get { return _address; }
            set {
                if (_address != value) {
                    _address = value;
                    OnPropertyChanged();
                }
            }
        }

        public LocationViewModel Location {
            get { return _location; }
            set {
                if (_location != value) {
                    _location = value;
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
                  (Name != null && Name.ToLower().Contains(search)));
        }

        public override bool Filter(FilterColumnsViewModel filterColumns) {
            bool result = true;
            return result;
        }

        public  bool FilterDestinationWarehouse(FilterColumnsViewModel filterColumns) {
            var result = true;
            foreach (var column in filterColumns.Columns) {
                if (column.Value == null) continue;
                switch (column.Column) {
                    case FilterColumnEnum.DestinationWarehouse:
                        result = result && Name.ToLower().Contains(column.Value.ToLower());
                        break;
                }
            }
            return result;
        }

        public  bool FilterWarehouse(FilterColumnsViewModel filterColumns) {
            var result = true;
            foreach (var column in filterColumns.Columns) {
                if (column.Value == null) continue;
                switch (column.Column) {
                    case FilterColumnEnum.Warehouse:
                        result = result && Name.ToLower().Contains(column.Value.ToLower());
                        break;
                }
            }
            return result;
        }
    }
}
