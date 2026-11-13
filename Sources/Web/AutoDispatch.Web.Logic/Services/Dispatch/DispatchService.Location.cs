using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Helpers;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ Location Changed ]

    private static bool CheckChanges(LocationDto location1, LocationDto location2) {
        if (location1 != null && location2 == null) return true;
        if (location1 == null && location2 != null) return true;
        if (location1 == null && location2 == null) return false;
        return MathHelper.LocationCompare(location1.Latitude, location2.Latitude) == false ||
               MathHelper.LocationCompare(location1.Longitude, location2.Longitude) == false;
    }

    /// <summary>
    ///     Save Reverse </summary>
    /// <remarks>Trailer -> Loads -> Truck -> Driver</remarks>
    public async Task<LocationDriverChangedResponseData> LocationDriverChanged(LocationDriverChangedRequestData request) {
        // предполагаю, что EventNotificationAsync запускает LoadNameOfLocationAsync(..) из за отсутствие подготовленного кэша
        await this.LoadNameOfLocationAsync(request.Location);

        Driver driver = await GetByIdAsync<Driver>(request.DriverId);
        if (driver != null) {
            int? historyLocationTruckId = null;
            int? historyLocationTrailerId = null;
            var truck = await GetByIdAsync<Truck>(driver.TruckId);
            if (truck != null) {
                var trailer = await GetByIdAsync<Trailer>(truck.TrailerId);
                if (trailer != null) {
                    await _modifyLocationAsync(trailer, request.Location, request.UserId);
                    var tmp1 = await this._saveEventHistoryTrailerAsync(EventTypeModel.HistoryTrailer, trailer, request.UserId);
                    historyLocationTrailerId = tmp1.historyLocationTrailerId;
                    foreach (Load load in tmp1.loads) {
                        await _modifyLocationAsync(load, request.Location, request.UserId);
                        var tmp2 = await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load, historyLocationTrailerId, LoadParentType.Trailer, request.UserId);
                        foreach (Load load2 in tmp2.loads) {
                            await _modifyLocationAsync(load2, request.Location, request.UserId);
                            await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load2, tmp2.historyLocationLoadId, LoadParentType.Shipment, request.UserId);
                        }
                    }
                }
                await _modifyLocationAsync(truck, request.Location, request.UserId);
                var tmp3 = await this._saveEventHistoryTruckAsync(EventTypeModel.HistoryTruck, truck, historyLocationTrailerId, request.UserId);
                historyLocationTruckId = tmp3.historyLocationTruckId;
            }
            await _modifyLocationAsync(driver, request.Location, request.UserId);
            await this._saveEventHistoryDriverAsync(EventTypeModel.HistoryDriver, driver, historyLocationTruckId, request.UserId);

            var dto = this.Mapper.Map<DriverDto>(driver);
            dto.CurrentLocation = request.Location;
            return new LocationDriverChangedResponseData() {
                Driver = dto
            };
        }
        throw new InfoException($"Driver was not found: {request.DriverId}");
    }

    public async Task<LocationTruckChangedResponseData> LocationTruckChanged(LocationTruckChangedRequestData request) {
        await this.LoadNameOfLocationAsync(request.Location);

        Truck truck = await GetByIdAsync<Truck>(request.TruckId);
        if (truck != null) {
            int? historyLocationTrailerId = null;
            var trailer = await GetByIdAsync<Trailer>(truck.TrailerId);
            if (trailer != null) {
                LocationDto location = LocationConverter.ToDto(trailer.CurrentLatitude, trailer.CurrentLongitude);
                if (CheckChanges(location, request.Location)) {
                    await _modifyLocationAsync(trailer, request.Location, request.UserId);
                    var tmp1 = await this._saveEventHistoryTrailerAsync(EventTypeModel.HistoryTrailer, trailer, request.UserId);
                    historyLocationTrailerId = tmp1.historyLocationTrailerId;
                    foreach (Load load in tmp1.loads) {
                        await _modifyLocationAsync(load, request.Location, request.UserId);
                        var tmp2 = await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load, historyLocationTrailerId, LoadParentType.Trailer, request.UserId);
                        foreach (Load load2 in tmp2.loads) {
                            await _modifyLocationAsync(load2, request.Location, request.UserId);
                            await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load2, tmp2.historyLocationLoadId, LoadParentType.Shipment, request.UserId);
                        }
                    }
                }
                await _modifyLocationAsync(truck, request.Location, request.UserId);
                await this._saveEventHistoryTruckAsync(EventTypeModel.HistoryTruck, truck, historyLocationTrailerId, request.UserId);
            }
            var dto = this.Mapper.Map<TruckDto>(truck);
            dto.CurrentLocation = request.Location;
            return new LocationTruckChangedResponseData() {
                Truck = dto
            };
        }
        throw new InfoException($"Driver was not found: {request.TruckId}");
    }

    public async Task<LocationTrailerChangedResponseData> LocationTrailerChanged(LocationTrailerChangedRequestData request) {
        await this.LoadNameOfLocationAsync(request.Location);

        Trailer trailer = await GetByIdAsync<Trailer>(request.TrailerId);
        if (trailer != null) {
            LocationDto location = LocationConverter.ToDto(trailer.CurrentLatitude, trailer.CurrentLongitude);
            if (CheckChanges(location, request.Location)) {
                await _modifyLocationAsync(trailer, request.Location, request.UserId);
                var tmp1 = await this._saveEventHistoryTrailerAsync(EventTypeModel.HistoryTrailer, trailer, request.UserId);
                foreach (Load load in tmp1.loads) {
                    await _modifyLocationAsync(load, request.Location, request.UserId);
                    var tmp2 = await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load, tmp1.historyLocationTrailerId, LoadParentType.Trailer, request.UserId);
                    foreach (Load load2 in tmp2.loads) {
                        await _modifyLocationAsync(load2, request.Location, request.UserId);
                        // V1 не ждал этот вызов (fire-and-forget) — здесь ждём
                        await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load2, tmp2.historyLocationLoadId, LoadParentType.Shipment, request.UserId);
                    }
                }
            }
            var dto = this.Mapper.Map<TrailerDto>(trailer);
            dto.CurrentLocation = request.Location;
            return new LocationTrailerChangedResponseData() {
                Trailer = dto
            };
        }
        throw new InfoException($"Trailer was not found: {request.TrailerId}");
    }

    public async Task<LocationLoadChangedResponseData> LocationLoadChanged(LocationLoadChangedRequestData request) {
        await this.LoadNameOfLocationAsync(request.Location);

        Load load = await GetByIdAsync<Load>(request.LoadId);
        if (load != null) {
            LocationDto location = LocationConverter.ToDto(load.CurrentLatitude, load.CurrentLongitude);
            if (CheckChanges(location, request.Location)) {
                await _modifyLocationAsync(load, request.Location, request.UserId);
                var tmp1 = await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load, null, LoadParentType.None, request.UserId);
                foreach (Load load2 in tmp1.loads) {
                    await _modifyLocationAsync(load2, request.Location, request.UserId);
                    await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load2, tmp1.historyLocationLoadId, LoadParentType.Shipment, request.UserId);
                }
            }
            var dto = this.Mapper.Map<LoadDto>(load);
            dto.CurrentLocation = request.Location;
            return new LocationLoadChangedResponseData() {
                Load = dto
            };
        }
        throw new InfoException($"Load was not found: {request.LoadId}");
    }

    public async Task<LocationDatDriverChangedResponseData> LocationDatDriverChanged(LocationDatDriverChangedRequestData request) {
        await this.LoadNameOfLocationAsync(request.Location);

        DatDriver driver = await GetByIdAsync<DatDriver>(request.DatDriverId);
        if (driver != null) {
            await _modifyLocationAsync(driver, request.Location, request.UserId);
            var tmp1 = await this._saveEventHistoryDatDriverAsync(EventTypeModel.HistoryDatDriver, driver, request.UserId);
            foreach (Load load in tmp1.loads) {
                await _modifyLocationAsync(load, request.Location, request.UserId);
                var tmp2 = await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load, tmp1.historyLocationDatDriverId, LoadParentType.DatDriver, request.UserId);
                foreach (Load load2 in tmp2.loads) {
                    await _modifyLocationAsync(load2, request.Location, request.UserId);
                    await this._saveEventHistoryLoadAsync(EventTypeModel.HistoryBookedLoad, load2, tmp2.historyLocationLoadId, LoadParentType.Shipment, request.UserId);
                }
            }

            var dto = this.Mapper.Map<DatDriverDto>(driver);
            dto.CurrentLocation = request.Location;
            return new LocationDatDriverChangedResponseData() {
                DatDriver = dto
            };
        }
        throw new InfoException($"DAT driver was not found: {request.DatDriverId}");
    }

    #endregion [ Location Changed ]

    #region [ Load History Location ]

    public async Task<LoadHistoryObjectsResponseData> LoadHistoryObjects(LoadHistoryObjectsRequestData request) {
        var models = await Table<HistoryObject>().Where(h => request.Ids.Contains(h.Id)).ToListAsync();
        return new LoadHistoryObjectsResponseData {
            Histories = this.Mapper.Map<List<HistoryObjectDto>>(models)
        };
    }

    public async Task<LoadHistoryLocationDriverResponseData> LoadHistoryLocationDriver(LoadHistoryLocationDriverRequestData request) {
        DateTime dt = DateTime.UtcNow.AddSeconds(-request.DateTime.TimeOfDay.TotalSeconds);
        var list = await Table<HistoryLocationDriver>().Where(p => p.DriverId == request.DriverId && p.CreatedDate > dt)
            .OrderByDescending(m => m.CreatedDate).ToListAsync();
        var history = this.Mapper.Map<List<HistoryLocationDriverDto>>(list);
        foreach (var h in history) {
            await this.LoadNameOfLocationAsync(h);
        }
        return new LoadHistoryLocationDriverResponseData { History = history };
    }

    public async Task<LoadHistoryLocationTruckResponseData> LoadHistoryLocationTruck(LoadHistoryLocationTruckRequestData request) {
        DateTime dt = DateTime.UtcNow.AddSeconds(-request.DateTime.TimeOfDay.TotalSeconds);
        var list = await Table<HistoryLocationTruck>().Where(p => p.TruckId == request.TruckId && p.CreatedDate > dt)
            .OrderByDescending(m => m.CreatedDate).ToListAsync();
        var history = this.Mapper.Map<List<HistoryLocationTruckDto>>(list);
        foreach (var h in history) {
            await this.LoadNameOfLocationAsync(h);
        }
        return new LoadHistoryLocationTruckResponseData { History = history };
    }

    public async Task<LoadHistoryLocationTrailerResponseData> LoadHistoryLocationTrailer(LoadHistoryLocationTrailerRequestData request) {
        DateTime dt = DateTime.UtcNow.AddSeconds(-request.DateTime.TimeOfDay.TotalSeconds);
        var list = await Table<HistoryLocationTrailer>().Where(p => p.TrailerId == request.TrailerId && p.CreatedDate > dt)
            .OrderByDescending(m => m.CreatedDate).ToListAsync();
        var history = this.Mapper.Map<List<HistoryLocationTrailerDto>>(list);
        foreach (var h in history) {
            await this.LoadNameOfLocationAsync(h);
        }
        return new LoadHistoryLocationTrailerResponseData { History = history };
    }

    public async Task<LoadHistoryLocationLoadResponseData> LoadHistoryLocationLoad(LoadHistoryLocationLoadRequestData request) {
        // NOTE: skips a filter by Date, because a load has few locations
        var list = await Table<HistoryLocationLoad>().Where(p => p.LoadId == request.LoadId)
            .OrderByDescending(m => m.CreatedDate).ToListAsync();
        var history = this.Mapper.Map<List<HistoryLocationLoadDto>>(list);
        foreach (var h in history) {
            await this.LoadNameOfLocationAsync(h);
        }
        return new LoadHistoryLocationLoadResponseData { History = history };
    }

    public async Task<LoadHistoryLocationDatDriverResponseData> LoadHistoryLocationDatDriver(LoadHistoryLocationDatDriverRequestData request) {
        DateTime dt = DateTime.UtcNow.AddSeconds(-request.DateTime.TimeOfDay.TotalSeconds);
        var list = await Table<HistoryLocationDatDriver>().Where(p => p.DatDriverId == request.DatDriverId && p.CreatedDate > dt)
            .OrderByDescending(m => m.CreatedDate).ToListAsync();
        var history = this.Mapper.Map<List<HistoryLocationDatDriverDto>>(list);
        foreach (var h in history) {
            await this.LoadNameOfLocationAsync(h);
        }
        return new LoadHistoryLocationDatDriverResponseData { History = history };
    }

    public async Task<LoadHistoryChangesLoadResponseData> LoadHistoryChangesLoad(LoadHistoryChangesLoadRequestData request) {
        var load = await GetByIdAsync<Load>(request.LoadId);
        var objects = await Table<HistoryObject>().Where(e => e.ReferenceId == request.LoadId).ToListAsync();
        var history = this.Mapper.Map<List<HistoryObjectDto>>(objects);
        // addes a history of parent
        if (load?.ParentId != null) {
            var objects2 = await Table<HistoryObject>().Where(e => e.ReferenceId == load.ParentId).ToListAsync();
            var history2 = this.Mapper.Map<List<HistoryObjectDto>>(objects2);
            history = history.Concat(history2).OrderBy(e => e.CreatedDate).ToList();
        }
        return new LoadHistoryChangesLoadResponseData { History = history };
    }

    #endregion [ Load History Location ]
}
