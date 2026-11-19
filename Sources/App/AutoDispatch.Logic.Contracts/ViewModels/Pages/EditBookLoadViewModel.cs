using System.Collections.Generic;
using AutoCall.Common.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class EditBookLoadViewModel : ViewModel {
        /// <summary>
        ///     root </summary>
        public LoadViewModel Load { get; set; }
        /// <summary>
        ///     chindren </summary>
        public ObservableCollection<WrapperLoadViewModel> Loads { get; set; } = new ObservableCollection<WrapperLoadViewModel>();

        public IList<WarehouseViewModel> Warehouses { get; set; }
        public IList<TrailerViewModel> Trailers { get; set; }
        public IList<TruckViewModel> Trucks { get; set; }
        public IList<DriverViewModel> Drivers { get; set; }
        public IList<DatCompanyViewModel> DatCompanies { get; set; }
        public IList<DatDriverViewModel> DatDrivers { get; set; }
        public IList<ActualCompanyViewModel> ActualCompanies { get; }
        public ActualCompanyViewModel ActualCompany { get; }

        public void LineHaulChanged() {
            if (Load.LineHaul == null) {
                Loads.ForEach(e => e.Load.LineHaul = null);
            } else {
                Helpers.DispatchHelper.SetRatio(this.Load, this.Loads.Select(e => e.Load));
            }
        }

        public EditBookLoadViewModel(LoadViewModel load, IList<ActualCompanyViewModel> actualCompanies, ActualCompanyViewModel actualCompany) {
            this.Load = load;
            // Example: BrokerAccountingType to BrokerDispatch
            this.ActualCompanies = actualCompanies.Where(e => (int)e.BoardType == (int)load.BoardType % 2).ToList();
            this.ActualCompany = actualCompany;

            Load.LineHaulChanged = LineHaulChanged;
        }
    }
}
