using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Раз в 6 часов удаляет из БД логов строки старше сроков хранения (AppSettings.*RetainedDays), включая Metric </summary>
public class RetentionService : BaseBackgroundService {

    protected override TimeSpan Interval => TimeSpan.FromHours(6);
    protected override string Name => "retention";

    public RetentionService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        await host.GetService<LoginService>().CleanupAsync();
        await host.GetService<MetricService>().CleanupAsync();
    }
}
