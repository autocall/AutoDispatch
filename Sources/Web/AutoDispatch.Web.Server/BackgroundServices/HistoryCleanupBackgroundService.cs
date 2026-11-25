using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 QuartzJob (01:00 по будням): чистка HistoryObject/Event старше HistoryExpiresDays (CacheLoadJob не перенесён — таблицы CacheLoadIndex нет, AI/wiki/plan.md) </summary>
public class HistoryCleanupBackgroundService : BaseBackgroundService {

    /// <summary>
    ///     Время до ближайшего HistoryCleanupTimeUtc </summary>
    protected override TimeSpan Interval {
        get {
            var now = DateTime.UtcNow;
            var next = now.Date + AppSettings.HistoryCleanupTimeUtc;
            return (next > now ? next : next.AddDays(1)) - now;
        }
    }
    protected override string Name => "history-cleanup";

    public HistoryCleanupBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        await host.GetService<HistoryCleanupService>().DoAsync();
    }
}
