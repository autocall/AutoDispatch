using System.Collections.Generic;
using System.Windows;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.Logic.Enums;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents view model for main window </summary>
    public class MainViewModel : BaseViewModel {
        /// <summary>
        ///     Visible if debug </summary>
        public static Visibility IsDebug {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }

        /// <summary>
        ///     Boards settings </summary>
        private Dictionary<BoardType, BoardSettingsViewModel> _boardsSettings { get; set; }

        /// <summary>
        ///     Is data loading now </summary>
        private bool _isLoading;

        private List<object> _brokerCompanies;
        private List<object> _carrierCompanies;
        private object _selectedBrokerCompany;
        private object _selectedCarrierCompany;
        private bool _viewType;
        private bool _expanded;
        private int? _total;
        private int volume;
        private bool volumeMute;
        private bool allowHold;

        public InboundCallViewModel InboundCall { get; set; } = new InboundCallViewModel();

        public bool AllowHold {
            get => allowHold;
            set {
                allowHold = value;
                OnPropertyChanged();
            }
        }

        public int Volume {
            get { return volume; }
            set {
                volume = value;
                OnPropertyChanged();
            }
        }

        public bool VolumeMute {
            get { return volumeMute; }
            set {
                volumeMute = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets boards settings </summary>
        public Dictionary<BoardType, BoardSettingsViewModel> BoardsSettings {
            get { return _boardsSettings; }
            set {
                _boardsSettings = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     True: Dynamic, False: Classic </summary>
        public bool ViewType {
            get { return _viewType; }
            set {
                if (_viewType != value) {
                    _viewType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Total of created loads. Using only for dispatch </summary>
        public int? Total {
            get => _total;
            set {
                if (_total != value) {
                    _total = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets is data loading now </summary>
        public bool IsLoading {
            get { return _isLoading; }
            set {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        ///     Gets or sets is data of a expanding menu </summary>
        public bool Expanded {
            get { return _expanded; }
            set {
                _expanded = value;
                OnPropertyChanged();
            }
        }

        // ******* Only for dispatch *******
        public List<object> BrokerCompanies {
            get { return _brokerCompanies; }
            set {
                _brokerCompanies = value;
                OnPropertyChanged();
            }
        }

        public List<object> CarrierCompanies {
            get { return _carrierCompanies; }
            set {
                _carrierCompanies = value;
                OnPropertyChanged();
            }
        }

        public object SelectedBrokerCompany {
            get { return _selectedBrokerCompany; }
            set {
                if (_selectedBrokerCompany != value) {
                    _selectedBrokerCompany = value;
                    OnPropertyChanged();
                }
            }
        }

        public object SelectedCarrierCompany {
            get { return _selectedCarrierCompany; }
            set {
                if (_selectedCarrierCompany != value) {
                    _selectedCarrierCompany = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}