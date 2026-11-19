using AutoCall.Logic.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoCall.Logic.ViewModels {
    public class MicroSIPSettingsViewModel : BaseViewModel, ICloneable {
        public ObservableCollection<MicroSIPContactViewModel> Contacts { get; set; } = new ObservableCollection<MicroSIPContactViewModel>();
        public ObservableCollection<MicroSIPShortcutViewModel> Shortcuts { get; set; } = new ObservableCollection<MicroSIPShortcutViewModel>();

        public object Clone() {
            return new MicroSIPSettingsViewModel() {
                Contacts = new ObservableCollection<MicroSIPContactViewModel>(this.Contacts.Select(e => (MicroSIPContactViewModel)e.Clone())),
                Shortcuts = new ObservableCollection<MicroSIPShortcutViewModel>(this.Shortcuts.Select(e => (MicroSIPShortcutViewModel)e.Clone())),
            };
        }
    }

    public class MicroSIPShortcutViewModel : BaseViewModel, ICloneable {
        private int _index;
        private bool _hasErrorLabel;
        private bool _hasErrorNumber;

        [JsonIgnore]
        public int Index {
            get => _index;
            set {
                _index = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public bool HasErrorLabel {
            get => _hasErrorLabel;
            set {
                _hasErrorLabel = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public bool HasErrorNumber {
            get => _hasErrorNumber;
            set {
                _hasErrorNumber = value;
                OnPropertyChanged();
            }
        }
        public string Label { get; set; }
        public string Number { get; set; }
        public MicroSIPShortcutType Type { get; set; }

        public object Clone() {
            return new MicroSIPShortcutViewModel() {
                Index = this.Index,
                Number = this.Number,
                Label = this.Label,
                Type = this.Type,
            };
        }
    }

    public class MicroSIPContactViewModel : BaseViewModel, ICloneable {
        private int _index;
        private bool _hasErrorName;
        private bool _hasErrorNumber;

        [JsonIgnore]
        public int Index {
            get => _index;
            set {
                _index = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public bool HasErrorName {
            get => _hasErrorName;
            set {
                _hasErrorName = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public bool HasErrorNumber {
            get => _hasErrorNumber;
            set {
                _hasErrorNumber = value;
                OnPropertyChanged();
            }
        }
        public string Name { get; set; }
        public string Number { get; set; }

        public object Clone() {
            return new MicroSIPContactViewModel() {
                Index = this.Index,
                Name = this.Name,
                Number = this.Number,
            };
        }
    }
}
