using System.Collections.Concurrent;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Dtos.Base;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Mapping.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Вся логика App (V1 DispatchService, 4651 строка): грузы, водители, техника, карриеры, история и события, файлы, письма.
///     Код V1 перенесён по частям (partial): EF6 с change tracking → LinqToDB с явными Insert/Update по PK, geography → decimal-пары.
///     Ошибки для пользователя — InfoException (V1 DispatchException) </summary>
public partial class DispatchService : DispatchBaseService, IDispatchService {

    private static readonly TimeSpan ReconnectInterval = AppSettings.EventWaitTimeout;

    private DatCompanyService DatCompanies => Host.GetService<DatCompanyService>();
    private GeocodeService Geocode => Host.GetService<GeocodeService>();

    public DispatchService(ServicesHost host) : base(host) { }

    #region [ CurrentEventId ]

    private static readonly ConcurrentDictionary<Guid, int> _companyToEvent = new ConcurrentDictionary<Guid, int>();

    public static void RemoveCompanyCache(Guid companyId) {
        _companyToEvent.TryRemove(companyId, out _);
    }

    private async Task<int> _getCurrentEventIdAsync(Guid companyId) {
        if (_companyToEvent.TryGetValue(companyId, out var value) == false) {
            int? eventId = await Table<Event>().Where(e => e.CompanyId == companyId).MaxAsync(e => (int?)e.Id);
            value = _companyToEvent[companyId] = eventId ?? 0;
        }
        return value;
    }

    private void _setCurrentEventId(Guid companyId, int eventId) {
        _companyToEvent[companyId] = eventId;
    }

    /// <summary>
    ///     Новое событие компании (identity Id), кэш последнего Id обновляется — его ждёт EventNotificationAsync </summary>
    private async Task<Event> InsertEventAsync(Guid companyId, EventTypeModel type, int? historyId) {
        var @event = new Event {
            CompanyId = companyId,
            Type = (byte)type,
            HistoryId = historyId,
            CreatedDate = DateTime.UtcNow,
        };
        @event.Id = await Db.InsertWithInt32IdentityAsync(@event);
        _setCurrentEventId(companyId, @event.Id);
        return @event;
    }

    #endregion [ CurrentEventId ]

    public async Task<GetModulesResponseData> GetModules(GetModulesRequestData request) {
        var exists = await Table<AdUser>().AnyAsync(e => e.Id == request.UserId);
        if (exists == false) {
            throw new InfoException("User was Undefined");
        }
        List<ModuleDto> modules = new List<ModuleDto> {
            new ModuleDto { Title = "Chat", News = true },
            new ModuleDto { Title = "LogBook", News = false },
            new ModuleDto { Title = "Dispatcher", News = false },
            new ModuleDto { Title = "Settings", News = false }
        };
        return new GetModulesResponseData {
            Modules = modules
        };
    }

    /// <summary>
    ///     Коллеги по компании (V1 User_ModelToDto: Name → UserName, RoleId — первая роль Identity) </summary>
    public async Task<GetUsersResponseData> GetUsers(GetUsersRequestData request) {
        var companyId = await GetCompanyIdByUserIdAsync(request.UserId);
        var users = await (
            from u in Table<AdUser>()
            join ur in Table<AdUserRole>() on u.Id equals ur.UserId into urs
            from ur in urs.DefaultIfEmpty()
            where u.CompanyId == companyId && u.Id != request.UserId && u.IsDeleted == false
            select new { u.Id, u.Email, u.UserName, u.PhoneNumber, RoleId = ur == null ? Guid.Empty : ur.RoleId }
        ).ToListAsync();
        return new GetUsersResponseData {
            Users = users.GroupBy(e => e.Id).Select(g => g.First()).Select(e => new UserDispatchDto {
                Id = e.Id,
                Email = e.Email,
                Name = e.UserName,
                Phone = e.PhoneNumber,
                PhotoUrl = "",
                RoleId = e.RoleId
            }).ToList()
        };
    }

    public ProviderEnabledResponseData ProviderEnabled(ProviderEnabledRequestData request) {
        return new ProviderEnabledResponseData();
    }

    public ProviderDisabledResponseData ProviderDisabled(ProviderDisabledRequestData request) {
        return new ProviderDisabledResponseData();
    }

    #region [ Save History ]

    private async Task _saveEventHistoryDriverAsync(EventTypeModel eventType, Driver driver, int? historyLocationTruckId, Guid userId) {
        var @event = await InsertEventAsync(driver.CompanyId, eventType, driver.HistoryId);
        var entity = new HistoryLocationDriver {
            EventId = @event.Id,
            HistoryId = driver.HistoryId,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow,
            DriverId = driver.EntityId,
            HistoryLocationTruckId = historyLocationTruckId,
            Latitude = driver.CurrentLatitude,
            Longitude = driver.CurrentLongitude
        };
        await Db.InsertWithInt32IdentityAsync(entity);
    }

    private async Task<(Trailer trailer, int historyLocationTruckId)> _saveEventHistoryTruckAsync(
            EventTypeModel eventType, Truck truck, int? historyLocationTrailerId, Guid userId) {
        var @event = await InsertEventAsync(truck.CompanyId, eventType, truck.HistoryId);
        var historyLocationTruck = new HistoryLocationTruck {
            EventId = @event.Id,
            HistoryId = truck.HistoryId,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow,
            TruckId = truck.EntityId,
            HistoryLocationTrailerId = historyLocationTrailerId,
            Latitude = truck.CurrentLatitude,
            Longitude = truck.CurrentLongitude
        };
        historyLocationTruck.Id = await Db.InsertWithInt32IdentityAsync(historyLocationTruck);
        var trailer = await GetByIdAsync<Trailer>(truck.TrailerId);
        return (trailer, historyLocationTruck.Id);
    }

    private async Task<(ICollection<Load> loads, int historyLocationTrailerId)> _saveEventHistoryTrailerAsync(
        EventTypeModel eventType, Trailer trailer, Guid userId) {
        var @event = await InsertEventAsync(trailer.CompanyId, eventType, trailer.HistoryId);
        var historyLocationTrailer = new HistoryLocationTrailer {
            EventId = @event.Id,
            HistoryId = trailer.HistoryId,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow,
            TrailerId = trailer.EntityId,
            Latitude = trailer.CurrentLatitude,
            Longitude = trailer.CurrentLongitude
        };
        historyLocationTrailer.Id = await Db.InsertWithInt32IdentityAsync(historyLocationTrailer);
        var loads = await Alive<Load>().Where(e => (e.Board == 0 || e.Board == 1) && e.TrailerId == trailer.EntityId).ToListAsync();
        return (loads, historyLocationTrailer.Id);
    }

    private async Task<ICollection<Load>> _saveEventWarehouseAsync(Warehouse entity, Guid userId) {
        await InsertEventAsync(entity.CompanyId, EventTypeModel.Warehouse, entity.HistoryId);
        return await Alive<Load>().Where(e => (e.Board == 0 || e.Board == 1) && e.WarehouseId == entity.EntityId).ToListAsync();
    }

    private async Task<ICollection<Trailer>> _saveEventYardAsync(Yard entity, Guid userId) {
        await InsertEventAsync(entity.CompanyId, EventTypeModel.Yard, entity.HistoryId);
        return await Table<Trailer>().Where(e => e.YardId == entity.EntityId).ToListAsync();
    }

    /// <summary>
    ///     Returns children loads. Parent has ShipmentId </summary>
    private async Task<(ICollection<Load> loads, int historyLocationLoadId)> _saveEventHistoryLoadAsync(
            EventTypeModel eventType, Load load, int? historyParentId, LoadParentType parentType, Guid userId) {
        var @event = await InsertEventAsync(load.CompanyId, eventType, load.HistoryId);
        var entity = new HistoryLocationLoad {
            EventId = @event.Id,
            HistoryId = load.HistoryId,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow,
            LoadId = load.EntityId,
            Latitude = load.CurrentLatitude,
            Longitude = load.CurrentLongitude
        };
        switch (parentType) {
            case LoadParentType.Trailer:
                entity.HistoryLocationTrailerId = historyParentId;
                break;
            case LoadParentType.DatDriver:
                entity.HistoryLocationDatDriverId = historyParentId;
                break;
            case LoadParentType.DatCompany:
                entity.HistoryDatCompanyId = historyParentId;
                break;
            case LoadParentType.Shipment:
                entity.HistoryShipmentId = historyParentId;
                break;
            case LoadParentType.Warehouse:
                entity.HistoryWarehouseId = historyParentId;
                break;
        }
        entity.Id = await Db.InsertWithInt32IdentityAsync(entity);

        var status = (byte)LoadStatus.InTransit;
        var children = await Table<Load>().Where(l => l.ShipmentId == load.EntityId && l.Status == status).ToListAsync();
        return (children, entity.Id);
    }

    private async Task _saveEventHistoriesLoadsAsync(
            EventTypeModel eventType, Dictionary<Load, int?> loadsParentIds, LoadParentType parentType, Guid userId) {
        foreach (KeyValuePair<Load, int?> loadParentId in loadsParentIds) {
            var load = loadParentId.Key;
            var @event = await InsertEventAsync(load.CompanyId, eventType, load.HistoryId);
            var entity = new HistoryLocationLoad {
                EventId = @event.Id,
                HistoryId = load.HistoryId,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow,
                LoadId = load.EntityId,
                Latitude = load.CurrentLatitude,
                Longitude = load.CurrentLongitude
            };
            switch (parentType) {
                case LoadParentType.Trailer:
                    entity.HistoryLocationTrailerId = loadParentId.Value;
                    break;
                case LoadParentType.DatDriver:
                    entity.HistoryLocationDatDriverId = loadParentId.Value;
                    break;
                case LoadParentType.DatCompany:
                    entity.HistoryDatCompanyId = loadParentId.Value;
                    break;
                case LoadParentType.Shipment:
                    entity.HistoryShipmentId = loadParentId.Value;
                    break;
                case LoadParentType.Warehouse:
                    entity.HistoryWarehouseId = loadParentId.Value;
                    break;
            }
            await Db.InsertWithInt32IdentityAsync(entity);
        }
    }

    private async Task _saveEventSetupPacketAsync(SetupPacket entity, Guid userId) {
        await InsertEventAsync(entity.CompanyId, EventTypeModel.SetupPacket, entity.HistoryId);
    }

    private async Task<ICollection<Load>> _saveEventDatCompanyAsync(DatCompany entity, Guid userId) {
        await InsertEventAsync(entity.CompanyId, EventTypeModel.DatCompany, entity.HistoryId);
        return await Alive<Load>().Where(e => (e.Board == 0 || e.Board == 1) && e.DatCompanyId == entity.EntityId).ToListAsync();
    }

    private async Task<(ICollection<Load> loads, int historyLocationDatDriverId)> _saveEventHistoryDatDriverAsync(
            EventTypeModel eventType, DatDriver entity, Guid userId) {
        var @event = await InsertEventAsync(entity.CompanyId, eventType, entity.HistoryId);
        var historyLocationDatDriver = new HistoryLocationDatDriver {
            EventId = @event.Id,
            HistoryId = entity.HistoryId,
            CreatedById = userId,
            CreatedDate = DateTime.UtcNow,
            DatDriverId = entity.EntityId,
            Latitude = entity.CurrentLatitude,
            Longitude = entity.CurrentLongitude
        };
        historyLocationDatDriver.Id = await Db.InsertWithInt32IdentityAsync(historyLocationDatDriver);
        var loads = await Table<Load>().Where(e => e.DatDriverId == entity.EntityId).ToListAsync();
        return (loads, historyLocationDatDriver.Id);
    }

    #endregion [ Save History ]

    /// <summary>
    ///     Текущее положение объекта (V1 _modifyLocation; изменения EF сохранял при следующем SaveChanges — здесь сразу) </summary>
    private async Task _modifyLocationAsync(BaseDispatchEntity entity, LocationDto location, Guid userId) {
        LocationConverter.SetCurrent(entity, location);
        entity.LastModifiedById = userId;
        entity.LastModifiedDate = DateTime.UtcNow;
        await UpdateAsync(entity);
    }

    #region [ LoadNameOfLocation ]

    public async Task LoadNameOfLocationAsync(BaseDispatchDto dto) {
        if (dto.CurrentLocation != null) {
            try {
                dto.CurrentLocation.FormattedAddress = await Geocode.GetFormattedPlaceNameAsync(dto.CurrentLocation);
            } catch (Exception e) {
                if (e.Message.Contains("ZERO_RESULTS") == false) {
                    _l.e($"LoadNameOfLocation[1]\n{dto.CurrentLocation.Latitude}, {dto.CurrentLocation.Longitude}", e);
                }
            }
        }
    }

    public async Task LoadNameOfLocationAsync(LocationDto dto) {
        if (dto != null) {
            try {
                dto.FormattedAddress = await Geocode.GetFormattedPlaceNameAsync(dto);
            } catch (Exception e) {
                if (e.Message.Contains("ZERO_RESULTS") == false) {
                    _l.e("LoadNameOfLocation[3]", e);
                }
            }
        }
    }

    public async Task LoadNameOfLocationAsync(BaseEventLocationDto dto) {
        await LoadNameOfLocationAsync(dto.Location);
    }

    /// <summary>
    ///     Bulk load </summary>
    public async Task LoadNameOfLocationAsync<T>(IList<T> dtos) where T : BaseDispatchDto {
        try {
            var locations = dtos.Where(e => e.CurrentLocation != null).Select(e => e.CurrentLocation).ToList();
            await Geocode.GetFormattedPlaceNamesAsync(locations);
        } catch (Exception e) {
            if (e.Message.Contains("ZERO_RESULTS") == false) {
                _l.e("LoadNameOfLocation[3]", e);
            }
        }
    }

    #endregion [ LoadNameOfLocation ]
}
