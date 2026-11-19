using System.Collections.Generic;
using System.Threading.Tasks;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels {
    public delegate Task DriversReceivedHandler(IList<DriverViewModel> drivers);
    public delegate Task TrucksReceivedHandler(IList<TruckViewModel> trucks);
    public delegate Task TrailersReceivedHandler(IList<TrailerViewModel> trailers);
    public delegate Task WarehousesReceivedHandler(IList<WarehouseViewModel> warehouses);
    public delegate Task YardsReceivedHandler(IList<YardViewModel> yards);
    public delegate Task LoadsReceivedHandler(IList<LoadViewModel> loads);
    public delegate Task DatCompaniesReceivedHandler(IList<DatCompanyViewModel> companies);
    public delegate Task DatDriversReceivedHandler(IList<DatDriverViewModel> drivers);
    public delegate Task SetupPacketsReceivedHandler(IList<SetupPacketViewModel> setupPacket);
}
