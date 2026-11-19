namespace AutoDispatch.Logic.ViewModels.Items {

    public class WrapperLoadViewModel : ViewModel {
        private bool _isExpanded;
        private bool _bound = true;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    base.OnPropertyChanged();
                }
            }
        }
        public bool Bound {
            get { return _bound; }
            set {
                if (_bound != value) {
                    _bound = value;
                    base.OnPropertyChanged();
                }
            }
        }

        public LoadViewModel Load { get; private set; }

        public WrapperLoadViewModel(LoadViewModel load) {
            Load = load;
        }
    }
}
