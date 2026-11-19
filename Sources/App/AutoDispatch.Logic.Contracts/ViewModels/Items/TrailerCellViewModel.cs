using System;
using AutoCall.Common.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class TrailerCellViewModel : ViewModel, ICloneable {
        public int Index { get; }
        public ObservableCollection<LoadPalletViewModel> Pallets { get; private set; }

        public int RowIndex {
            get { return Index / TrailerViewModel.MaxColumns; }
        }
        public int ColumnIndex {
            get { return Index % TrailerViewModel.MaxColumns; }
        }

        public TrailerCellViewModel(int index) {
            Pallets = new ObservableCollection<LoadPalletViewModel>();
            Index = index;
        }

        public void UpdateCell() {
            OnPropertyChanged("Pallets");
        }

        public object Clone() {
            var clone = new TrailerCellViewModel(this.Index);
            CopyTo(clone);
            return clone;
        }

        /// <summary>
        ///     Copies data from another record </summary>
        /// <param name="from">Source record</param>
        public virtual void CopyFrom(TrailerCellViewModel from) {
            GetType()
                .GetProperties()
                .Where(p => p.CanWrite && p.CanRead)
                .ForEach(p => p.SetValue(this, p.GetValue(from, null), null));

            this.Pallets = new ObservableCollection<LoadPalletViewModel>();
            foreach (var pallet in from.Pallets) {
                this.Pallets.Add((LoadPalletViewModel) pallet.Clone());
            }
        }

        /// <summary>
        ///     Copies data to another record </summary>
        /// <param name="to">Destination record</param>
        public virtual void CopyTo(TrailerCellViewModel to) {
            GetType()
                .GetProperties()
                .Where(p => p.CanWrite && p.CanRead)
                .ForEach(p => p.SetValue(to, p.GetValue(this, null), null));

            to.Pallets = new ObservableCollection<LoadPalletViewModel>();
            foreach (var pallet in this.Pallets) {
                to.Pallets.Add((LoadPalletViewModel)pallet.Clone());
            }
        }
    }
}
