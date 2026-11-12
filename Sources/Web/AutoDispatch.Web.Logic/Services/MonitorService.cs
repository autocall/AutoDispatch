using System.Diagnostics;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic.Dtos.Log;
using AutoDispatch.Web.Logic.Monitoring;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Данные для /Admin/Monitor: пинг обеих БД, онлайн-пользователи </summary>
public class MonitorService : BaseService {

    private OnlineTracker Tracker => Host.ServiceProvider.GetRequiredService<OnlineTracker>();

    public MonitorService(ServicesHost host) : base(host) { }

    public async Task<List<DbPingDto>> PingAsync() {
        return [
            await PingAsync(UnitOfWork.Context, AppSettings.DataBaseName),
            await PingAsync(UnitOfWork.LogContext, AppSettings.LogDataBaseName)
        ];
    }

    public List<OnlineUserDto> GetOnline() {
        return Tracker.GetOnline().Select(e => new OnlineUserDto {
            UserId = e.UserId,
            CompanyId = e.CompanyId,
            Email = e.Email,
            Source = e.Source,
            LastSeen = e.LastSeen
        }).ToList();
    }

    #region [ private ]

    private static async Task<DbPingDto> PingAsync(DataConnection db, string name) {
        var sw = Stopwatch.StartNew();
        try {
            await db.ExecuteAsync<int>("select 1");
            return new DbPingDto { Name = name, Ok = true, Milliseconds = sw.Elapsed.TotalMilliseconds };
        } catch (Exception e) {
            return new DbPingDto { Name = name, Ok = false, Milliseconds = sw.Elapsed.TotalMilliseconds, Error = e.GetBaseException().Message };
        }
    }

    #endregion [ private ]
}
