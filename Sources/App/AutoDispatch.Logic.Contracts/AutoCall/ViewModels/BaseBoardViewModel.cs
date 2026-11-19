using AutoCall.Logic.Enums;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents base view model for board </summary>
    public class BaseBoardViewModel : BaseViewModel {

        public static /*GridsViewSettingsViewModel*/object _gridsSettings;
        public /*GridsViewSettingsViewModel*/object GridsSettings { get => _gridsSettings; set => _gridsSettings = value; }

        /// <summary>
        ///     Worker state </summary>
        private WorkerState _workerState;

        /// <summary>
        ///     Gets or sets worker state </summary>
        public WorkerState WorkerState {
            get { return _workerState; }
            set {
                _workerState = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor </summary>
        public BaseBoardViewModel()
            : base() {
            _workerState = WorkerState.Stopped;
        }
    }
}