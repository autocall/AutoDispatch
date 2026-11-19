using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SearchDriverViewModel : ViewModel {
        private string _searchText;
        private Wrapper3DriverViewModel _selectedWrapper;

        public List<DriverViewModel> Drivers { get; set; }
        public List<TruckViewModel> Trucks { get; set; }
        public List<TrailerViewModel> Trailers { get; set; }
        public List<Wrapper3DriverViewModel> Wrappers { get; set; }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public Wrapper3DriverViewModel SelectedWrapper {
            get { return _selectedWrapper; }
            set {
                if (_selectedWrapper != value) {
                    _selectedWrapper = value;
                    OnPropertyChanged();
                }
            }
        }

        public SearchDriverViewModel() {
            Drivers = new List<DriverViewModel>();
            Trucks = new List<TruckViewModel>();
            Trailers = new List<TrailerViewModel>();
            Wrappers = new List<Wrapper3DriverViewModel>();
        }

        public void BuildWrappers() {
            foreach (var driver in Drivers) {
                var wrapper = new Wrapper3DriverViewModel();
                Wrappers.Add(wrapper);
                wrapper.Driver = driver;
                if (driver.TruckId != null) {
                    wrapper.Truck = Trucks.FirstOrDefault(t => t.EntityId == driver.TruckId);
                    if (wrapper.Truck != null && wrapper.Truck.TrailerId != null) {
                        wrapper.Trailer =Trailers.FirstOrDefault(t => t.EntityId == wrapper.Truck.TrailerId);
                    }
                }
            }
        }
    }
}
