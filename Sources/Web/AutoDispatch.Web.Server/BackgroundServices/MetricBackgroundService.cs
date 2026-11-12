using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Раз в MetricFlushInterval снимает CPU веб-сервера и сохраняет закрытые минуты в Metric (как Safer); DB CPU приходит в Api/Metrics/Add </summary>
public class MetricBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.MetricFlushInterval;
    protected override string Name => "metrics";

    public MetricBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        var service = host.GetService<MetricService>();
        await service.MeasureCpuAsync(stoppingToken);
        await service.SaveAsync();
    }
}
