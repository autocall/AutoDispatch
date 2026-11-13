using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 LoadWaitingJob (5 мин): грузам с просроченным pickup ставит статус Waiting </summary>
public class LoadWaitingBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.LoadWaitingInterval;
    protected override string Name => "load-waiting";

    public LoadWaitingBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        return host.GetService<DispatchService>().LoadSetWaitingStatusAsync();
    }
}
