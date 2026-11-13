using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Repositories;

public interface IRepository { }

public interface IRepository<T> : IRepository {

    /// <summary>
    ///     Все сущности, IsDeleted отсекается если includeIsDeleted = false </summary>
    IQueryable<T> GetAll(bool includeIsDeleted);

    /// <summary>
    ///     Gets entity by PK Id </summary>
    Task<T> GetByIdAsync(Guid id);

    /// <summary>
    ///     Insert: Id (если 0), Created*/Modified* проставляются здесь </summary>
    Task InsertAsync(T model, Guid userId);

    /// <summary>
    ///     Bulk insert </summary>
    Task InsertAsync(IEnumerable<T> models, Guid userId);

    /// <summary>
    ///     Update только свойств TMap плюс Modified* </summary>
    Task<int> UpdateAsync<TMap>(T newModel, Guid userId) where TMap : IBaseUpdateMap;

    /// <summary>
    ///     Soft delete / restore </summary>
    Task<int> UpdateIsDeletedAsync(Guid id, bool value, Guid userId);

    /// <summary>
    ///     Физическое удаление по Id </summary>
    Task<int> DeleteAsync(Guid id);
}
