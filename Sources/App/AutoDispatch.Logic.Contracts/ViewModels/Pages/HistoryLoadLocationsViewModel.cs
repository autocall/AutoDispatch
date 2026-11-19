using System.Collections.ObjectModel;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class HistoryLoadLocationsViewModel : ViewModel {
        private ObservableCollection<HistoryLoadViewModel> _histories = 
            new ObservableCollection<HistoryLoadViewModel>();

        public ObservableCollection<HistoryLoadViewModel> Histories {
            get { return _histories; }
            set {
                if (_histories != value) {
                    _histories = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoadViewModel Load { get; set; }
    }

    public class HistoryLoadViewModel : ViewModel {
        private HistoryLocationLoadViewModel _historyLocation;
        private LoadDto _history;

        public HistoryLocationLoadViewModel HistoryLocation {
            get { return _historyLocation; }
            set {
                if (_historyLocation != value) {
                    _historyLocation = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoadDto History {
            get { return _history; }
            set {
                if (_history != value) {
                    _history = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
