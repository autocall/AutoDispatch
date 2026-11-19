using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class EquipmentsViewModel : ViewModel {
        public ObservableCollection<TruckViewModel> Trucks { get; set; } = new ObservableCollection<TruckViewModel>();
        public ObservableCollection<TrailerViewModel> Trailers { get; set; } = new ObservableCollection<TrailerViewModel>();
        public ObservableCollection<YardViewModel> Yards { get; set; } = new ObservableCollection<YardViewModel>();

        private string _searchTextTruck;
        public string SearchTextTruck {
            get { return _searchTextTruck; }
            set {
                if (_searchTextTruck != value) {
                    _searchTextTruck = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _searchTextTrailer;
        public string SearchTextTrailer {
            get { return _searchTextTrailer; }
            set {
                if (_searchTextTrailer != value) {
                    _searchTextTrailer = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
