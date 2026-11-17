using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 QuartzJob (01:00 по будням): чистка HistoryObject/Event старше HistoryExpiresDays (CacheLoadJob не перенесён — таблицы CacheLoadIndex нет, AI/wiki/plan.md) </summary>
public class HistoryCleanupBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.HistoryCleanupInterval;
    protected override string Name => "history-cleanup";

    public HistoryCleanupBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        await host.GetService<HistoryCleanupService>().DoAsync();
    }
}
