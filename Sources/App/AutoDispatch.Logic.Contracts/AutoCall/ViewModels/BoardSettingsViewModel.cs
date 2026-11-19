using System;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents board settings view model </summary>
    public class BoardSettingsViewModel : BaseViewModel {
        /// <summary>
        ///     Is board enabled </summary>
        private bool _enabled;

        private double _zoomValue = 1;
        
        /// <summary>
        ///     Gets or sets is board enabled </summary>
        public bool Enabled {
            get { return _enabled; }
            set {
                if (_enabled != value) {
                    _enabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public double ZoomValue {
            get { return _zoomValue; }
            set {
                if (Math.Abs(_zoomValue - value) > Double.Epsilon) {
                    _zoomValue = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}