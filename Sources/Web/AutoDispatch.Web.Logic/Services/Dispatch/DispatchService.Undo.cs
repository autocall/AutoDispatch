using LinqToDB;
using LinqToDB.Async;
using AutoCall.Common.Dtos;
using AutoDispatch.Dtos.Data;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

public partial class DispatchService {

    #region [ Undo ]

    public async Task<UndoPreviewResponseData> UndoPreview(UndoPreviewRequestData request) {
        BaseDto dto1 = request.Type switch {
            EventTypeModel.BookedLoad => (await GetHistoryEntityAsync<Load, LoadDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.Driver => (await GetHistoryEntityAsync<Driver, DriverDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.Truck => (await GetHistoryEntityAsync<Truck, TruckDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.Trailer => (await GetHistoryEntityAsync<Trailer, TrailerDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.Warehouse => (await GetHistoryEntityAsync<Warehouse, WarehouseDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.Yard => (await GetHistoryEntityAsync<Yard, YardDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.DatCompany => (await GetHistoryEntityAsync<DatCompany, DatCompanyDto>(request.EntityId, request.UserId, false)).dto,
            EventTypeModel.DatDriver => (await GetHistoryEntityAsync<DatDriver, DatDriverDto>(request.EntityId, request.UserId, false)).dto,
            _ => throw new InfoException($"Undo is not supported for {request.Type}")
        };
        return new UndoPreviewResponseData() { Dto = dto1 };
    }

    public async Task<UndoApplyResponseData> UndoApply(UndoApplyRequestData request) {
        switch (request.Type) {
            case EventTypeModel.BookedLoad: {
                    var result = await GetHistoryEntityAsync<Load, LoadDto>(request.EntityId, request.UserId, true);
                    if (!result.result) {
                        return new UndoApplyResponseData();
                    }
                    await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, result.entity, null, LoadParentType.None, request.UserId);
                    if (result.groupId != null) {
                        List<Load> entities = await GetHistoryEntitiesAsync<Load, LoadDto>(request.UserId, (Guid)result.groupId);
                        foreach (var entity2 in entities) {
                            await this._saveEventHistoryLoadAsync(EventTypeModel.BookedLoad, entity2, null, LoadParentType.None, request.UserId);
                        }
                    }
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.Driver: {
                    var result = await GetHistoryEntityAsync<Driver, DriverDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventHistoryDriverAsync(EventTypeModel.Driver, result.entity, null, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.Truck: {
                    var result = await GetHistoryEntityAsync<Truck, TruckDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventHistoryTruckAsync(EventTypeModel.Truck, result.entity, null, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.Trailer: {
                    var result = await GetHistoryEntityAsync<Trailer, TrailerDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventHistoryTrailerAsync(EventTypeModel.Trailer, result.entity, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.Warehouse: {
                    var result = await GetHistoryEntityAsync<Warehouse, WarehouseDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventWarehouseAsync(result.entity, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.Yard: {
                    var result = await GetHistoryEntityAsync<Yard, YardDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventYardAsync(result.entity, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.DatCompany: {
                    var result = await GetHistoryEntityAsync<DatCompany, DatCompanyDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventDatCompanyAsync(result.entity, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            case EventTypeModel.DatDriver: {
                    var result = await GetHistoryEntityAsync<DatDriver, DatDriverDto>(request.EntityId, request.UserId, true);
                    if (!result.result) return new UndoApplyResponseData();
                    await this._saveEventHistoryDatDriverAsync(EventTypeModel.DatDriver, result.entity, request.UserId);
                    return new UndoApplyResponseData() { Dto = result.dto };
                }
            default:
                throw new InfoException($"Undo is not supported for {request.Type}");
        }
    }

    /// <summary>
    ///     Предыдущий снимок сущности; apply — откатить (снимки после него помечаются Undo) </summary>
    /// <returns>groupId — текущий</returns>
    private async Task<(bool result, TDto dto, TEntity entity, Guid? groupId)> GetHistoryEntityAsync<TEntity, TDto>(Guid entityId, Guid userId, bool apply)
        where TEntity : BaseObjectEntity where TDto : BaseDto {
        var entity = await GetByIdAsync<TEntity>(entityId);
        var historyId = entity.HistoryId; // current history or last undo
        var current = await Table<HistoryObject>().FirstOrDefaultAsync(h => h.Id == historyId);
        var historyObject = await Table<HistoryObject>().Where(h => h.ReferenceId == entityId && h.Id < historyId && h.Undo == false)
            .OrderByDescending(h => h.Id).FirstOrDefaultAsync();
        if (historyObject != null) {
            var groupId = current?.GroupId;
            var dto = ZipExtension.DeserializeObject<TDto>(historyObject.Data);
            if (dto.CreatedById == Guid.Empty) {
                dto.CreatedById = entity.CreatedById;
                dto.CreatedDate = entity.CreatedDate;
            }
            if (dto.LastModifiedById == Guid.Empty) {
                dto.LastModifiedById = entity.LastModifiedById;
                dto.LastModifiedDate = entity.LastModifiedDate;
            }
            if (entity is Load load && dto is LoadDto loadDto) {
                if (load.ContainerType != (int)loadDto.ContainerType) {
                    throw new InfoException("you cannot undo the operation of creating sub-loads");
                }
            }
            if (apply) {
                await Table<HistoryObject>().Where(h => h.ReferenceId == entityId && h.Id >= historyId).Set(h => h.Undo, true).UpdateAsync();
                entity = Mapper.Map<TEntity>(dto);
                entity.HistoryId = historyObject.Id; // save history
                await UpdateAsync(entity);
            }
            return (true, dto, entity, groupId);
        }
        return (false, null, null, null);
    }

    /// <summary>
    ///     Using for Undo </summary>
    /// <param name="groupId">текущий. этот group Id надо пропустить и возвратить предыдущий</param>
    private async Task<List<TEntity>> GetHistoryEntitiesAsync<TEntity, TDto>(Guid userId, Guid groupId)
        where TEntity : BaseObjectEntity where TDto : BaseDto {
        var entities = new List<TEntity>();

        // вытаскиваем ReferenceIds в этом сейве
        List<HistoryObject> currenthistories = (await Table<HistoryObject>().Where(e => e.GroupId == groupId && e.Undo == false).ToListAsync())
            .GroupBy(e => e.ReferenceId) // если в группе несколько сохранений на одну сущность, то роли не играет, какую мы вернём
            .Select(e => e.First()).ToList();

        foreach (HistoryObject currentHistoryObject in currenthistories) {
            var historyObject = await Table<HistoryObject>()
                .Where(h => h.ReferenceId == currentHistoryObject.ReferenceId &&
                            h.Id < currentHistoryObject.Id &&
                            h.GroupId != groupId &&
                            h.Undo == false)
                .OrderByDescending(h => h.Id)
                .FirstOrDefaultAsync();
            if (historyObject != null) {
                await Table<HistoryObject>().Where(h => h.Id == historyObject.Id).Set(h => h.Undo, true).UpdateAsync();
                var dto = ZipExtension.DeserializeObject<TDto>(historyObject.Data);
                var entity = await GetByIdAsync<TEntity>(dto.EntityId);
                if (dto.CreatedById == Guid.Empty) {
                    dto.CreatedById = entity.CreatedById;
                    dto.CreatedDate = entity.CreatedDate;
                }
                if (dto.LastModifiedById == Guid.Empty) {
                    dto.LastModifiedById = entity.LastModifiedById;
                    dto.LastModifiedDate = entity.LastModifiedDate;
                }
                entity = Mapper.Map<TEntity>(dto);
                entity.HistoryId = historyObject.Id; // save history
                await UpdateAsync(entity);
                entities.Add(entity);
            }
        }
        return entities;
    }

    #endregion [ Undo ]
}
