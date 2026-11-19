
namespace AutoDispatch.Logic.ViewModels.Items {
    public class Wrapper3DriverViewModel : ViewModel {
        private DriverViewModel _driver;
        private TruckViewModel _truck;
        private TrailerViewModel _trailer;

        public DriverViewModel Driver {
            get { return _driver; }
            set {
                if (_driver != value) {
                    _driver = value;
                    OnPropertyChanged();
                }
            }
        }

        public TruckViewModel Truck {
            get { return _truck; }
            set {
                if (_truck != value) {
                    _truck = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrailerViewModel Trailer {
            get { return _trailer; }
            set {
                if (_trailer != value) {
                    _trailer = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
