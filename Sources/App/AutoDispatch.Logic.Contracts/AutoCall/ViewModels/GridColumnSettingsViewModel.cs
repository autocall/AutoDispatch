namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents view model for grid column settings </summary>
    public class GridColumnSettingsViewModel : BaseViewModel {
        /// <summary>
        ///     Is column enabled </summary>
        private bool _enabled;

        /// <summary>
        ///     Column width </summary>
        private int _width;

        /// <summary>
        ///     Column order index </summary>
        private int _index;

        /// <summary>
        ///     Gets or sets is column enabled </summary>
        public bool Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets column width </summary>
        public int Width {
            get { return _width; }
            set {
                _width = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets column order index </summary>
        public int Index {
            get { return _index; }
            set {
                _index = value;
                OnPropertyChanged();
            }
        }
    }
}