using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SearchTrailerViewModel : ViewModel {
        private string _searchText;
        private TrailerViewModel _selectedWrapper;
        public List<TrailerViewModel> Trailers { get; set; }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel SelectedWrapper {
            get { return _selectedWrapper; }
            set {
                if (_selectedWrapper != value) {
                    _selectedWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public SearchTrailerViewModel() {
            Trailers = new List<TrailerViewModel>();
        }
    }
}
