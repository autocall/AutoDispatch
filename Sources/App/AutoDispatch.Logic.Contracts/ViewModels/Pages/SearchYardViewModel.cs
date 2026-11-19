using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SearchYardViewModel : ViewModel {
        private string _searchText;
        private YardViewModel _selectedWrapper;
        public List<YardViewModel> Yards { get; set; }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public YardViewModel SelectedWrapper {
            get { return _selectedWrapper; }
            set {
                if (_selectedWrapper != value) {
                    _selectedWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public SearchYardViewModel() {
            Yards = new List<YardViewModel>();
        }
    }
}
