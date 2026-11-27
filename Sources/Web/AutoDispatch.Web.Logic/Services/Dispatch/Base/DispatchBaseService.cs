using System.Collections.Concurrent;
using System.Diagnostics;
using LinqToDB;
using LinqToDB.Async;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Dispatch;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Email;
using AutoDispatch.Web.Logic.Monitoring;

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

    /// <summary>
    ///     Запись по id; в запросе App чужая компания — AccessDeniedException (аудит D1) </summary>
    protected async Task<T> GetByIdAsync<T>(Guid id) where T : Data.Entities.Dispatch.BaseEntity {
        var entity = await Table<T>().FirstOrDefaultAsync(e => e.EntityId == id);
        if (entity is ICompanyEntity owned) {
            CheckCompany(owned.CompanyId, $"{typeof(T).Name} {id}");
        }
        return entity;
    }

    protected Task<T> GetByIdAsync<T>(Guid? id) where T : Data.Entities.Dispatch.BaseEntity {
        return id == null ? Task.FromResult<T>(null) : GetByIdAsync<T>(id.Value);
    }

    protected Task<int> InsertAsync<T>(T entity) where T : class => Db.InsertAsync(entity);

    protected Task<int> UpdateAsync<T>(T entity) where T : class => Db.UpdateAsync(entity);

    protected Task<int> DeleteAsync<T>(T entity) where T : class => Db.DeleteAsync(entity);

    #region [ email ]

    /// <summary>
    ///     Письмо через SMTP компании (EmailUtils) с метрикой SMTP; сбой — warning с сервером SMTP и исключение дальше, как раньше </summary>
    protected async Task SendEmailAsync(SmtpSettings smtp, Email.Email email) {
        var metrics = Host.ServiceProvider.GetRequiredService<MetricCollector>();
        var stopwatch = Stopwatch.StartNew();
        try {
            await EmailUtils.SendEmailAsync(smtp, email);
        } catch (Exception e) {
            metrics.AddExternal(MetricTypeEnum.SmtpLatency, stopwatch.Elapsed, false);
            _l.w($"SMTP {smtp.Host}:{smtp.Port} (user {smtp.UserName}) failed for '{email.Subject}': {e.GetBaseException().Message}");
            throw;
        }
        metrics.AddExternal(MetricTypeEnum.SmtpLatency, stopwatch.Elapsed, true);
    }

    #endregion [ email ]

    #region [ company of App request ]

    /// <summary>
    ///     Компания пользователя App (ServicesHost.DispatchCompanyId); null — серверный вызов без проверки </summary>
    protected Guid? RequestCompanyId => Host.DispatchCompanyId;

    /// <summary>
    ///     record — что запрошено, для лога («Load {id}»); клиенту уходит только «Access denied» </summary>
    protected void CheckCompany(Guid companyId, string record) {
        if (RequestCompanyId != null && companyId != RequestCompanyId) {
            _l.w($"Access denied: {record} of company {companyId}, user company {RequestCompanyId}");
            throw new AccessDeniedException();
        }
    }

    #endregion [ company of App request ]

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
