using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 BnsfRailwayJob (30 мин): отчёт BNSF по компаниям с BnsfLogin. В V1 выключен системным флагом (никто не пользуется с 2024),
///     здесь — AppSettings.BnsfRefreshEnabled </summary>
public class BnsfRefreshBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.BnsfRefreshInterval;
    protected override string Name => "bnsf-refresh";

    public BnsfRefreshBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        return AppSettings.BnsfRefreshEnabled ? host.GetService<DispatchService>().BnsfRefreshes() : Task.CompletedTask;
    }
}
