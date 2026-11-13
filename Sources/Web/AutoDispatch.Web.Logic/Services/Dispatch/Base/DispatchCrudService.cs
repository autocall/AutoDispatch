using LinqToDB;
using LinqToDB.Async;
using AutoCall.Common.Dtos;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     CRUD по DTO для сущностей Entities.Dispatch (V1 EntityCrudService): вставка или полное обновление строки
///     с сохранением Created*; удаление — IsDeleted. Наследники: CustomerCompany, ShipperReceiverCompany, DatCompany, DispatchCompany, SetupPacket </summary>
public abstract class DispatchCrudService<TDto, TEntity> : DispatchBaseService
    where TDto : BaseDto
    where TEntity : Data.Entities.Dispatch.BaseEntity {

    protected DispatchCrudService(ServicesHost host) : base(host) { }

    public virtual async Task<TDto> GetByIdAsync(Guid id) {
        var model = await GetByIdAsync<TEntity>(id);
        return Mapper.Map<TDto>(model);
    }

    public virtual async Task<TDto> GetByIdAsync(Guid? id) {
        return id == null ? null : await GetByIdAsync(id.Value);
    }

    public virtual async Task<List<TDto>> GetAllAsync() {
        var models = await Alive<TEntity>().ToListAsync();
        return Mapper.Map<List<TDto>>(models);
    }

    public virtual async Task<List<TDto>> SearchForAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) {
        var models = await Alive<TEntity>().Where(predicate).ToListAsync();
        return Mapper.Map<List<TDto>>(models);
    }

    /// <summary>
    ///     Save model to database (V1 EntityCrudService.SaveAsync) </summary>
    public virtual async Task SaveAsync(TDto dto, Guid userId) {
        var model = await GetByIdAsync<TEntity>(dto.EntityId);
        if (model == null) {
            model = Mapper.Map<TEntity>(dto);
            if (model.EntityId == Guid.Empty) {
                dto.EntityId = model.EntityId = Guid.NewGuid();
            }
            model.CreatedById = model.LastModifiedById = userId;
            model.CreatedDate = model.LastModifiedDate = DateTime.UtcNow;
            await InsertAsync(model);
        } else {
            var newModel = Mapper.Map<TEntity>(dto);
            newModel.CreatedById = model.CreatedById;
            newModel.CreatedDate = model.CreatedDate;
            newModel.LastModifiedById = userId;
            newModel.LastModifiedDate = DateTime.UtcNow;
            OnUpdate(model, newModel);
            await UpdateAsync(newModel);
        }
    }

    /// <summary>
    ///     Поля, которые при обновлении берутся из БД, если DTO их не прислал (пароли) </summary>
    protected virtual void OnUpdate(TEntity old, TEntity @new) { }

    public virtual async Task MarkAsDeletedIdAsync(Guid id, Guid userId) {
        await Table<TEntity>().Where(e => e.EntityId == id)
            .Set(e => e.IsDeleted, true)
            .Set(e => e.LastModifiedById, userId)
            .Set(e => e.LastModifiedDate, DateTime.UtcNow)
            .UpdateAsync();
    }

    public virtual async Task DeleteIdAsync(Guid id) {
        await Table<TEntity>().Where(e => e.EntityId == id).DeleteAsync();
    }
}
