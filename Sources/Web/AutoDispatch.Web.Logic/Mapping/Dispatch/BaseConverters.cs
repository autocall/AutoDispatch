using AutoMapper;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Logic.Helpers.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     Конвертеры V1 (Converters/Dispatch/**) на ITypeConverter AutoMapper 13: ConvertCore(source) как TypeConverter V1.
///     DispatchMappingProfile регистрирует их по namespace через reflection </summary>
public abstract class TypeConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination> {

    protected ResolutionContext Context { get; private set; }

    public TDestination Convert(TSource source, TDestination destination, ResolutionContext context) {
        this.Context = context;
        return source == null ? default : ConvertCore(source);
    }

    protected abstract TDestination ConvertCore(TSource source);
}

public abstract class Base_DtoToModel<TBaseDto, TBaseEntity> : TypeConverter<TBaseDto, TBaseEntity>
    where TBaseDto : BaseDto
    where TBaseEntity : BaseEntity {
    protected override TBaseEntity ConvertCore(TBaseDto source) {
        var destination = Activator.CreateInstance<TBaseEntity>();
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
        return destination;
    }
}

public abstract class Base_ModelToDto<TBaseEntity, TBaseDto> : TypeConverter<TBaseEntity, TBaseDto>
    where TBaseEntity : BaseEntity
    where TBaseDto : BaseDto {
    protected override TBaseDto ConvertCore(TBaseEntity source) {
        var destination = Activator.CreateInstance<TBaseDto>();
        destination.EntityId = source.EntityId;
        destination.CreatedById = source.CreatedById;
        destination.CreatedByName = UserNameCache.Get(source.CreatedById);
        destination.CreatedDate = source.CreatedDate;
        destination.LastModifiedById = source.LastModifiedById;
        destination.LastModifiedByName = UserNameCache.Get(source.LastModifiedById);
        destination.LastModifiedDate = source.LastModifiedDate;
        destination.IsDeleted = source.IsDeleted;
        return destination;
    }
}

public abstract class BaseObject_DtoToModel<TBaseObjectDto, TBaseObjectEntity> : Base_DtoToModel<TBaseObjectDto, TBaseObjectEntity>
    where TBaseObjectDto : BaseObjectDto
    where TBaseObjectEntity : BaseObjectEntity {
    protected override TBaseObjectEntity ConvertCore(TBaseObjectDto source) {
        var destination = base.ConvertCore(source);
        destination.CompanyId = source.CompanyId;
        destination.HistoryId = source.HistoryId;
        return destination;
    }
}

public abstract class BaseObject_ModelToDto<TBaseObjectEntity, TBaseObjectDto> : Base_ModelToDto<TBaseObjectEntity, TBaseObjectDto>
    where TBaseObjectEntity : BaseObjectEntity
    where TBaseObjectDto : BaseObjectDto {
    protected override TBaseObjectDto ConvertCore(TBaseObjectEntity source) {
        var destination = base.ConvertCore(source);
        destination.CompanyId = source.CompanyId;
        destination.HistoryId = source.HistoryId;
        return destination;
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
