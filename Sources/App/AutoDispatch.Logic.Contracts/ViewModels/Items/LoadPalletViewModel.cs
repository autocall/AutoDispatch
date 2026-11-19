using AutoCall.Common.Interfaces;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class LoadPalletViewModel : ViewModel, IAssignable {

        #region [ _fields ]

        private bool _changed;
        private byte _level;
        private byte _index;

        #endregion [ _fields ]

        public LoadViewModel Load { get; set; }

        public byte Index {
            get { return _index; }
            set {
                if (_index != value) {
                    _index = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte Level {
            get { return _level; }
            set {
                if (_level != value) {
                    _level = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Changed {
            get { return _changed; }
            set {
                if (_changed != value) {
                    _changed = value;
                    OnPropertyChanged();
                }
            }
        }

        public object Trigger { get; set; }

        public void UpdateTrigger() {
            OnPropertyChanged("Trigger");
        }

        public LoadPalletViewModel(LoadViewModel load) {
            Load = load;
        }

        /// <summary>
        ///     Clone </summary>
        /// <remarks>Here implemented custom method then Load has reference to this. Occured call overflow</remarks>
        public virtual object Clone() {
            var clone = new LoadPalletViewModel(Load);
            clone.Index = Index;
            clone.Level = Level;
            clone.Trigger = Trigger;
            return clone;
        }

        public virtual void Assign(object source) {
            var pallet = (LoadPalletViewModel)source;
            Index = pallet.Index;
            Level = pallet.Level;
            Trigger = pallet.Trigger;
        }
    }
}
