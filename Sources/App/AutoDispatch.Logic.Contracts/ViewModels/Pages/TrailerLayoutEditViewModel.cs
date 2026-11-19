using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class TrailerLayoutEditViewModel : ViewModel {
        public TrailerViewModel Trailer { get; set; }
        public IList<LoadViewModel> Loads { get; set; }
        public IList<LoadPalletViewModel> AddPallets { get; set; }
        public ObservableCollection<LoadPalletViewModel> TempPallets { get; set; }
    }
}
