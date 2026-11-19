using System.Collections.ObjectModel;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class TrailerLayoutPickupViewModel : ViewModel {
        public LoadViewModel Load { get; set; }
        public TrailerViewModel Trailer { get; set; }
        public ObservableCollection<LoadPalletViewModel> AvailablePallets { get; set; }
        public DivideType DivideType { get; set; }
    }
}
