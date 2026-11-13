using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 QuartzJob (01:00 по будням) + CacheLoadJob (00:10): чистка HistoryObject/Event старше HistoryExpiresDays и индекс грузов за сутки </summary>
public class HistoryCleanupBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.HistoryCleanupInterval;
    protected override string Name => "history-cleanup";

    public HistoryCleanupBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        await host.GetService<HistoryCleanupService>().DoAsync();
        await host.GetService<LoadIndexCacheService>().FixLoadIndexCacheAsync(1);
    }
}
