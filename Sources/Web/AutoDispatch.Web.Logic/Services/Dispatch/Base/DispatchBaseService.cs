using System.Collections.Concurrent;
using LinqToDB;
using LinqToDB.Async;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     База сервисов dispatch: таблицы Entities.Dispatch (своя BaseEntity с Guid EntityId, не BaseEntity V3 — репозитории V3
///     к ним не подходят) читаются и пишутся LinqToDB напрямую. Insert/Update — целиком по PK, как EF6 Update(entity) в V1 </summary>
public abstract class DispatchBaseService : BaseService {

    protected DispatchBaseService(ServicesHost host) : base(host) { }

    protected LinqToDB.Data.DataConnection Db => UnitOfWork.Context;

    protected ITable<T> Table<T>() where T : class => Db.GetTable<T>();

    /// <summary>
    ///     Живые записи (IsDeleted = false), как EntityRepository.GetAll V1 </summary>
    protected IQueryable<T> Alive<T>() where T : Data.Entities.Dispatch.BaseEntity => Table<T>().Where(e => e.IsDeleted == false);

    protected Task<T> GetByIdAsync<T>(Guid id) where T : Data.Entities.Dispatch.BaseEntity {
        return Table<T>().FirstOrDefaultAsync(e => e.EntityId == id);
    }

    protected Task<T> GetByIdAsync<T>(Guid? id) where T : Data.Entities.Dispatch.BaseEntity {
        return id == null ? Task.FromResult<T>(null) : GetByIdAsync<T>(id.Value);
    }

    protected Task<int> InsertAsync<T>(T entity) where T : class => Db.InsertAsync(entity);

    protected Task<int> UpdateAsync<T>(T entity) where T : class => Db.UpdateAsync(entity);

    protected Task<int> DeleteAsync<T>(T entity) where T : class => Db.DeleteAsync(entity);

    #region [ company by user ]

    private static readonly ConcurrentDictionary<Guid, Guid> _userToCompany = new ConcurrentDictionary<Guid, Guid>();

    public static void RemoveUserCache(Guid userId) {
        _userToCompany.TryRemove(userId, out _);
    }

    /// <summary>
    ///     Компания пользователя (кэш на процесс, как в V1) </summary>
    public async Task<Guid> GetCompanyIdByUserIdAsync(Guid userId) {
        if (_userToCompany.TryGetValue(userId, out var value) == false) {
            var companyId = await Table<AdUser>().Where(e => e.Id == userId).Select(e => (Guid?)e.CompanyId).FirstOrDefaultAsync();
            if (companyId == null) {
                throw new AccessDeniedException();
            }
            value = _userToCompany[userId] = companyId.Value;
        }
        return value;
    }

    #endregion [ company by user ]
}
