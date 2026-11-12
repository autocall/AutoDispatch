using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Monitoring;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Раз в ActivityFlushInterval пишет в UserActivity всех, кто делал запросы за интервал </summary>
public class ActivityFlushService : BaseBackgroundService {

    private readonly OnlineTracker _tracker;

    protected override TimeSpan Interval => AppSettings.ActivityFlushInterval;
    protected override string Name => "activity";

    public ActivityFlushService(IServiceProvider serviceProvider, OnlineTracker tracker) : base(serviceProvider) {
        this._tracker = tracker;
    }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        var entries = _tracker.GetSeenSince(DateTime.UtcNow - Interval);
        await host.GetService<LoginService>().InsertActivityAsync(entries);
        _tracker.Cleanup();
    }
}
