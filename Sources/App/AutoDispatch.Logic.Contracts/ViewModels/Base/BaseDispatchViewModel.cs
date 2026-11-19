using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels {

    public abstract class BaseDispatchViewModel : BaseObjectViewModel {
        private LocationViewModel _currentLocation;

        public LocationViewModel CurrentLocation {
            get { return _currentLocation; }
            set {
                if (_currentLocation != value) {
                    _currentLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        protected BaseDispatchViewModel() {
            CurrentLocation = new LocationViewModel();
        }
    }
}
