using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public partial class DispatchBoardViewModel {
        private ActualCompanyViewModel _selectedCarrierCompany;

        #region [ Public ]

        public ActualCompanyViewModel SelectedCarrierCompany {
            get { return _selectedCarrierCompany; }
            set {
                _selectedCarrierCompany = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Adds or updates trucks </summary>
        public async Task SetTrucksAsync(IList<TruckViewModel> trucks, bool refreshWrappers = true) {
            if (trucks == null) return;
            foreach (var truck in trucks) {
                var existsTruck = Trucks.FirstOrDefault(i => i.EntityId == truck.EntityId);
                if (existsTruck != null) {
                    if (truck.IsDeleted == false) {
                        // Replace
                        existsTruck.Assign(truck);
                    } else {
                        // Delete
                        Trucks.Remove(existsTruck);
                    }
                } else {
                    // Add
                    Trucks.Add(truck);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.Truck);
            UpdateMinEquipmentExpiration();
        }

        /// <summary>
        ///     Adds or updates Trailers </summary>
        public async Task SetTrailersAsync(IList<TrailerViewModel> trailers, bool refreshWrappers = true) {
            if (trailers == null) return;
            Debug.WriteLine($"SetTrailers({trailers.Count}) -->");
            Stopwatch sw0 = Stopwatch.StartNew();
            foreach (var trailer in trailers) {
                var existsTrailer = Trailers.FirstOrDefault(i => i.EntityId == trailer.EntityId);
                if (existsTrailer != null) {
                    if (trailer.IsDeleted == false) {
                        // Replace
                        existsTrailer.Assign(trailer);
                    } else {
                        // Delete
                        Trailers.Remove(existsTrailer);
                    }
                    // Update VIN on dispatch board
                } else {
                    // Add
                    Trailers.Add(trailer);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.Trailer);
            UpdateMinEquipmentExpiration();
            sw0.Stop();
            Debug.WriteLine($"SetTrailers({trailers.Count}) {sw0.Elapsed.TotalSeconds:F2}sec");
        }

        /// <summary>
        ///     Adds or updates Warehouses </summary>
        public async Task SetWarehousesAsync(IList<WarehouseViewModel> warehouses, bool refreshWrappers = true) {
            if (warehouses == null) return;
            foreach (var warehouse in warehouses) {
                var existsWarehouse = Warehouses.FirstOrDefault(i => i.EntityId == warehouse.EntityId);
                if (existsWarehouse != null) {
                    if (warehouse.IsDeleted == false) {
                        // Replace
                        existsWarehouse.Assign(warehouse);
                    } else {
                        // Delete
                        Warehouses.Remove(existsWarehouse);
                    }
                    // Update VIN on dispatch board
                } else {
                    // Add
                    Warehouses.Add(warehouse);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.Warehouse);
        }

        /// <summary>
        ///     Adds or updates Yards </summary>
        public async Task SetYardsAsync(IList<YardViewModel> yards, bool refreshWrappers = true) {
            if (yards == null) return;
            foreach (var yard in yards) {
                var existsYard = Yards.FirstOrDefault(i => i.EntityId == yard.EntityId);
                if (existsYard != null) {
                    if (yard.IsDeleted == false) {
                        // Replace
                        existsYard.Assign(yard);
                    } else {
                        // Delete
                        Yards.Remove(existsYard);
                    }
                } else {
                    // Add
                    Yards.Add(yard);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.Yard);
        }

        /// <summary>
        ///     Adds or updates drivers </summary>
        public async Task SetDriversAsync(IList<DriverViewModel> drivers, bool refreshWrappers = true) {
            if (drivers == null) return;
            foreach (var driver in drivers) {
                var existsDriver = Drivers.FirstOrDefault(i => i.EntityId == driver.EntityId);
                if (existsDriver != null) {
                    if (driver.IsDeleted == false) {
                        // Replace
                        existsDriver.Assign(driver);
                    } else {
                        // Delete
                        Drivers.Remove(existsDriver);
                    }
                } else {
                    // Add
                    Drivers.Add(driver);
                }
            }
            if (refreshWrappers)
                await RefreshWrappersAsync(EventTypeDto.Driver);
            UpdateMinDriverExpiration();
        }

        #endregion [ Public ]
    }
}
