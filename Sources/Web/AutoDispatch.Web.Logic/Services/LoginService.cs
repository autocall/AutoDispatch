using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities;
using AutoDispatch.Web.Data.Entities.Log;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Data.Helpers;
using AutoDispatch.Web.Logic.Monitoring;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Журнал входов и активность — БД логов. Ошибка записи не должна ломать вход: ловится и логируется </summary>
public class LoginService : BaseService {

    /// <summary>
    ///     Продукт в User-Agent AutoDispatch App (BaseApi, этап 6 плана) </summary>
    private const string AppUserAgentPrefix = "AutoDispatch/";

    public LoginService(ServicesHost host) : base(host) { }

    public async Task RecordAsync(string email, AcUser user, bool success, LoginSourceEnum source, string ip, string userAgent) {
        try {
            await UnitOfWork.LogContext.InsertAsync(new UserLogin {
                Id = DbRandomHelper.NewInt64(),
                DateTime = DateTime.UtcNow,
                UserId = user?.Id,
                CompanyId = user?.CompanyId,
                Email = email,
                Source = source,
                Success = success,
                Ip = ip,
                UserAgent = userAgent
            });
        } catch (Exception e) {
            _l.e("UserLogin insert failed", e);
        }
    }

    public async Task<List<UserLogin>> GetRecentAsync(int count) {
        return await UnitOfWork.LogContext.GetTable<UserLogin>().OrderByDescending(e => e.DateTime).Take(count).ToListAsync();
    }

    /// <summary>
    ///     Версия AutoDispatch App по пользователям — из User-Agent "AutoDispatch/1.0.1.9" последнего успешного входа App (AccountApi клиента) </summary>
    public async Task<Dictionary<int, string>> GetAppVersionsAsync() {
        var table = UnitOfWork.LogContext.GetTable<UserLogin>();
        var logins = table.Where(e => e.Success && e.Source == LoginSourceEnum.App && e.UserId != null);
        var query =
            from l in logins
            join last in logins.GroupBy(e => e.UserId).Select(g => new { UserId = g.Key, DateTime = g.Max(e => e.DateTime) })
                on new { l.UserId, l.DateTime } equals new { last.UserId, last.DateTime }
            select new { l.UserId, l.UserAgent };
        var rows = await query.ToListAsync();
        return rows.Where(e => e.UserAgent != null && e.UserAgent.StartsWith(AppUserAgentPrefix))
            .GroupBy(e => e.UserId.Value)
            .ToDictionary(g => g.Key, g => g.First().UserAgent.Substring(AppUserAgentPrefix.Length));
    }

    public async Task InsertActivityAsync(IEnumerable<OnlineTracker.Entry> entries) {
        var rows = entries.Select(e => new UserActivity {
            Id = DbRandomHelper.NewInt64(),
            UserId = e.UserId,
            CompanyId = e.CompanyId,
            SeenAt = e.LastSeen,
            Source = e.Source
        }).ToList();
        if (rows.Count > 0) {
            await UnitOfWork.LogContext.BulkCopyAsync(rows);
        }
    }

    /// <summary>
    ///     Чистка по срокам хранения из AppSettings </summary>
    public async Task CleanupAsync() {
        var db = UnitOfWork.LogContext;
        var logins = await db.GetTable<UserLogin>().Where(e => e.DateTime < DateTime.UtcNow.AddDays(-AppSettings.UserLoginRetainedDays)).DeleteAsync();
        var activity = await db.GetTable<UserActivity>().Where(e => e.SeenAt < DateTime.UtcNow.AddDays(-AppSettings.UserActivityRetainedDays)).DeleteAsync();
        var logs = await db.GetTable<LogEntry>().Where(e => e.TimeStamp < DateTime.UtcNow.AddDays(-AppSettings.LogRetainedDays)).DeleteAsync();
        if (logins + activity + logs > 0) {
            _l.i($"Retention: UserLogin {logins}, UserActivity {activity}, Log {logs} rows deleted");
        }
    }

}
