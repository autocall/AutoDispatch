using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Linq;
using AutoDispatch.Web.Data.Context;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Extensions;
using AutoDispatch.Web.Data.Helpers;

namespace AutoDispatch.Web.Data.Repositories;

public class LinqToDbRepository<T> : IRepository<T> where T : BaseEntity {

    protected readonly DataConnection Connection;

    protected virtual IQueryable<T> Table() {
        return Connection.GetTable<T>();
    }

    public LinqToDbRepository(LinqToDbContext dbContext) {
        this.Connection = dbContext;
    }

    public virtual IQueryable<T> GetAll(bool includeIsDeleted) {
        return includeIsDeleted ? Table() : Table().Where(e => e.IsDeleted == false);
    }

    public virtual async Task<T> GetByIdAsync(int id) {
        return await Table().FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task InsertAsync(T model, int userId) {
        Stamp(model, userId);
        await Connection.InsertAsync(model);
    }

    public virtual async Task InsertAsync(IEnumerable<T> models, int userId) {
        foreach (var model in models) {
            Stamp(model, userId);
        }
        await Connection.BulkCopyAsync(models);
    }

    public virtual async Task<int> UpdateAsync<TMap>(T newModel, int userId) where TMap : IBaseUpdateMap {
        IUpdatable<T> query = Table()
            .Where(e => e.Id == newModel.Id)
            .Set(e => e.ModifiedById, userId)
            .Set(e => e.ModifiedDateTime, DateTime.UtcNow);

        // GetProperties() интерфейса не включает унаследованные — собираем по всей цепочке, кроме IBaseUpdateMap (Modified* уже выставлены)
        var properties = typeof(TMap).GetInterfaces().Append(typeof(TMap))
            .Where(t => t != typeof(IBaseUpdateMap))
            .SelectMany(t => t.GetProperties())
            .DistinctBy(p => p.Name);
        foreach (var prop in properties) {
            query = query.Set(newModel, prop);
        }

        return await query.UpdateAsync();
    }

    public virtual async Task<int> UpdateIsDeletedAsync(int id, bool value, int userId) {
        return await Table()
            .Where(e => e.Id == id)
            .Set(e => e.IsDeleted, value)
            .Set(e => e.ModifiedById, userId)
            .Set(e => e.ModifiedDateTime, DateTime.UtcNow)
            .UpdateAsync();
    }

    public virtual async Task<int> DeleteAsync(int id) {
        return await Table().Where(e => e.Id == id).DeleteAsync();
    }

    #region [ private ]

    private static void Stamp(T model, int userId) {
        if (model.Id == default) {
            model.Id = DbRandomHelper.NewInt32();
        }
        model.CreatedById = userId;
        model.ModifiedById = userId;
        model.CreatedDateTime = DateTime.UtcNow;
        model.ModifiedDateTime = DateTime.UtcNow;
    }

    #endregion [ private ]
}
