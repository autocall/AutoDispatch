using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class DriversViewModel : ViewModel {
        public ObservableCollection<TruckViewModel> Trucks { get; set; } = new ObservableCollection<TruckViewModel>();
        public ObservableCollection<DriverViewModel> Drivers { get; set; } = new ObservableCollection<DriverViewModel>();
    }
}
