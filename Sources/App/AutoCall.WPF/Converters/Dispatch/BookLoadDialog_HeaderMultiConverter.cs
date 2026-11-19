using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Converters.Dispatch {
    public class BookLoadDialog_HeaderMultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            var load = values[0] as LoadViewModel;
            var datCompany = values[0] as DatCompanyViewModel;
            var datDriver = values[0] as DatDriverViewModel;
            var trailers = values[1] as IList<TrailerViewModel>;
            var trucks = values[2] as IList<TruckViewModel>;
            var drivers = values[3] as IList<DriverViewModel>;
            var datCompanies = values[4] as IList<DatCompanyViewModel>;
            var datDrivers = values[5] as IList<DatDriverViewModel>;
            var warehouses = values[5] as IList<WarehouseViewModel>;

            var result = new List<Inline>();
            if (warehouses != null && trailers != null && trucks != null && drivers != null && datDrivers != null && datCompanies != null) {
                if (load != null) {
                    if (load.TrailerId != null) {
                        var trailer = trailers.FirstOrDefault(t => t.EntityId == load.TrailerId);
                        if (trailer != null) {
                            var truck = trucks.FirstOrDefault(t => t.TrailerId == trailer.EntityId);
                            if (truck != null) {
                                var driver = drivers.FirstOrDefault(d => d.TruckId == truck.EntityId);
                                if (driver != null) {
                                    result.Add(new Run($"({driver.DisplayName})"));
                                } else {
                                    result.Add(new Run($"(truck: {truck.Number})"));
                                }
                            } else {
                                result.Add(new Run($"(trailer: {trailer.Number})"));
                            }
                        }
                    } else
                     if (load.WarehouseId != null) {
                        var warehouse = warehouses.FirstOrDefault(t => t.EntityId == load.WarehouseId.Value);
                        if (warehouse != null) {
                            result.Add(new Run($"({warehouse.Name})"));
                        } else {
                            result.Add(new Run($"(WAREHOUSE WAS NOT FOUND: {warehouse.Name})"));
                        }
                    } else
                    if (load.DatDriverId != null) {
                        var datDriver2 = datDrivers.FirstOrDefault(t => t.EntityId == load.DatDriverId.Value);
                        if (datDriver2 != null) {
                            result.Add(new Run($"({datDriver2.Name})"));
                        } else {
                            result.Add(new Run($"(DRIVER WAS NOT FOUND: {datDriver2.Name})"));
                        }
                    } else
                    if (load.DatCompanyId != null) {
                        var datCompany2 = datCompanies.FirstOrDefault(t => t.EntityId == load.DatCompanyId);
                        if (datCompany2 != null) {
                            result.Add(new Run($"({datCompany2.Name})"));
                        } else {
                            result.Add(new Run($"(COMPANY WAS NOT FOUND)"));
                        }
                    }
                } else {
                    result.Add(new Run($"(LOAD WAS NOT FOUND)"));
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
