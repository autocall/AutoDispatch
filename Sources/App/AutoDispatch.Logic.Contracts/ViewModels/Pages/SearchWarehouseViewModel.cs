using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SearchWarehouseViewModel : ViewModel {
        private string _searchText;
        private WarehouseViewModel _selectedWrapper;
        public List<WarehouseViewModel> Warehouses { get; set; }

        public bool Clearable { get; set; }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public WarehouseViewModel SelectedWrapper {
            get { return _selectedWrapper; }
            set {
                if (_selectedWrapper != value) {
                    _selectedWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public SearchWarehouseViewModel() {
            Warehouses = new List<WarehouseViewModel>();
        }
    }
}
