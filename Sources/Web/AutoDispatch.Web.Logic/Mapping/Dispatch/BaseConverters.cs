using AutoMapper;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     Общие поля конвертеров V1 (Converters/Dispatch/**): базовые классы Base_* / BaseObject_* стали методами Fill*.
///     Конвертеры — статические классы XConverter.ToModel/ToDto, в AutoMapper не регистрируются (решено 2026-11-18) </summary>
public static class BaseConverter {

    public static void FillModel(BaseDto source, BaseEntity destination) {
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
    }

    public static void FillDto(BaseEntity source, BaseDto destination) {
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedByName = UserNameCache.Get(source.CreatedById);
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedByName = UserNameCache.Get(source.LastModifiedById);
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
    }

    public static void FillObjectModel(BaseObjectDto source, BaseObjectEntity destination) {
        FillModel(source, destination);
        destination.CompanyId = source.CompanyId;
        destination.HistoryId = source.HistoryId;
    }

    public static void FillObjectDto(BaseObjectEntity source, BaseObjectDto destination) {
        FillDto(source, destination);
        destination.CompanyId = source.CompanyId;
        destination.HistoryId = source.HistoryId;
    }
}

/// <summary>
///     Конвертер по типу объекта для обобщённых методов DispatchService (SaveWithHistoryAsync, BaseDeleteAsync, Undo),
///     как V1 Object.DtoToVm в App: цепочка типов → конкретный XConverter </summary>
public static class ObjectConverter {

    public static TEntity ToModel<TEntity>(BaseObjectDto source) where TEntity : BaseObjectEntity {
        if (source == null) return null;
        BaseObjectEntity result = source switch {
            LoadDto dto => LoadConverter.ToModel(dto),
            DriverDto dto => DriverConverter.ToModel(dto),
            TruckDto dto => TruckConverter.ToModel(dto),
            TrailerDto dto => TrailerConverter.ToModel(dto),
            DatCompanyDto dto => DatCompanyConverter.ToModel(dto),
            DatDriverDto dto => DatDriverConverter.ToModel(dto),
            WarehouseDto dto => WarehouseConverter.ToModel(dto),
            YardDto dto => YardConverter.ToModel(dto),
            SetupPacketDto dto => SetupPacketConverter.ToModel(dto),
            _ => throw new Exception("Undefined type of BaseObjectDto: " + source.GetType())
        };
        return (TEntity)result;
    }

    public static TDto ToDto<TDto>(BaseObjectEntity source, IMapper mapper) where TDto : BaseObjectDto {
        if (source == null) return null;
        BaseObjectDto result = source switch {
            Load entity => LoadConverter.ToDto(entity),
            Driver entity => DriverConverter.ToDto(entity),
            Truck entity => TruckConverter.ToDto(entity),
            Trailer entity => TrailerConverter.ToDto(entity),
            DatCompany entity => DatCompanyConverter.ToDto(entity),
            DatDriver entity => DatDriverConverter.ToDto(entity),
            Warehouse entity => WarehouseConverter.ToDto(entity),
            Yard entity => YardConverter.ToDto(entity),
            SetupPacket entity => SetupPacketConverter.ToDto(entity, mapper),
            _ => throw new Exception("Undefined type of BaseObjectEntity: " + source.GetType())
        };
        return (TDto)result;
    }
}

/// <summary>
///     geography V1 → пары decimal (решено 2026-11-13): LocationDto ↔ Latitude/Longitude </summary>
public static class LocationConverter {

    public static LocationDto ToDto(decimal? latitude, decimal? longitude) {
        if (latitude == null || longitude == null) return null;
        return new LocationDto { Latitude = (float)latitude.Value, Longitude = (float)longitude.Value };
    }

    public static LocationDto ToDto(decimal latitude, decimal longitude) {
        return new LocationDto { Latitude = (float)latitude, Longitude = (float)longitude };
    }

    public static (decimal? Latitude, decimal? Longitude) ToModel(LocationDto location) {
        if (location == null || Single.IsNaN(location.Latitude) || Single.IsNaN(location.Longitude)) {
            return (null, null);
        }
        return ((decimal)location.Latitude, (decimal)location.Longitude);
    }

    public static void SetCurrent(BaseDispatchEntity entity, LocationDto location) {
        var (lat, lng) = ToModel(location);
        entity.CurrentLatitude = lat;
        entity.CurrentLongitude = lng;
    }
}
