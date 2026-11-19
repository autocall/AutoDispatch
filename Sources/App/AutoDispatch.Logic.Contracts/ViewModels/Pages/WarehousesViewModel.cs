using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class WarehousesViewModel : ViewModel {
        public ObservableCollection<WarehouseViewModel> Warehouses { get; set; } = new ObservableCollection<WarehouseViewModel>();
    }
}
