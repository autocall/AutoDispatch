using AutoDispatch.Web.Data.Entities;

namespace AutoDispatch.Web.Data.Repositories;

public interface IRepository { }

public interface IRepository<T> : IRepository {

    /// <summary>
    ///     Все сущности, IsDeleted отсекается если includeIsDeleted = false </summary>
    IQueryable<T> GetAll(bool includeIsDeleted);

    /// <summary>
    ///     Gets entity by PK Id </summary>
    Task<T> GetByIdAsync(int id);

    /// <summary>
    ///     Insert: Id (если 0), Created*/Modified* проставляются здесь </summary>
    Task InsertAsync(T model, int userId);

    /// <summary>
    ///     Bulk insert </summary>
    Task InsertAsync(IEnumerable<T> models, int userId);

    /// <summary>
    ///     Update только свойств TMap плюс Modified* </summary>
    Task<int> UpdateAsync<TMap>(T newModel, int userId) where TMap : IBaseUpdateMap;

    /// <summary>
    ///     Soft delete / restore </summary>
    Task<int> UpdateIsDeletedAsync(int id, bool value, int userId);

    /// <summary>
    ///     Физическое удаление по Id </summary>
    Task<int> DeleteAsync(int id);
}
