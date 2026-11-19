using System.Collections.Generic;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Converter from Load to Stops </summary>
    /// <summary> V1 Stops/VmToVm: остановки трейлера из груза </summary>
    public static class StopsConverter {
        public static List<StopViewModel> ToVm(LoadViewModel load) {
            var stops = new List<StopViewModel>();
            if (load.Status == LoadStatus.InTransit) {
                if (load.DeliveryOrder != null) {
                    stops.Add(new StopViewModel() {
                        ArriveDate = load.DeliveryDate,
                        ArriveFrom = load.DeliveryFrom,
                        ArriveTo = load.DeliveryTo,
                        Location = new LoadLocationViewModel() {
                            Address = load.DestinationAddress,
                            City = load.DestinationCity,
                            Location = load.DestinationLocation,
                        },
                        Load = load,
                        LoadId = load.EntityId,
                        Type = StopType.Delivery,
                        Order = load.DeliveryOrder.Value,
                    });
                }
            }
            if (load.Status == LoadStatus.AtPickUp) {
                if (load.PickupOrder != null) {
                    stops.Add(new StopViewModel() {
                        ArriveDate = load.PickupDate,
                        ArriveFrom = load.PickupFrom,
                        ArriveTo = load.PickupTo,
                        Location = new LoadLocationViewModel() {
                            Address = load.OriginAddress,
                            City = load.OriginCity,
                            Location = load.OriginLocation,
                        },
                        Load = load,
                        LoadId = load.EntityId,
                        Type = StopType.Pickup,
                        Order = load.PickupOrder.Value,
                    });
                }
                if (load.DeliveryOrder != null) {
                    stops.Add(new StopViewModel() {
                        ArriveDate = load.DeliveryDate,
                        ArriveFrom = load.DeliveryFrom,
                        ArriveTo = load.DeliveryTo,
                        Location = new LoadLocationViewModel() {
                            Address = load.DestinationAddress,
                            City = load.DestinationCity,
                            Location = load.DestinationLocation,
                        },
                        Load = load,
                        LoadId = load.EntityId,
                        Type = StopType.Delivery,
                        Order = load.DeliveryOrder.Value,
                    });
                }
            }
            return stops;
        }
    }
}
