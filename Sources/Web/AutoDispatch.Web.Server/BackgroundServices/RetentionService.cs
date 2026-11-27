using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Раз в 6 часов удаляет из БД логов строки старше сроков хранения (AppSettings.*RetainedDays), включая Metric.
///     По часам суток (00, 06, 12, 18 UTC), а не от старта: публикации чаще 6 часов обнуляли бы ожидание (аудит B5) </summary>
public class RetentionService : BaseBackgroundService {

    private static readonly TimeSpan Period = TimeSpan.FromHours(6);

    protected override TimeSpan Interval => UntilNextRun(DateTime.UtcNow);

    /// <summary>
    ///     Время до ближайшей границы Period от начала суток UTC </summary>
    public static TimeSpan UntilNextRun(DateTime utcNow) {
        var next = utcNow.Date + Period * (Math.Floor(utcNow.TimeOfDay / Period) + 1);
        return next - utcNow;
    }
    protected override string Name => "retention";

    public RetentionService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        await host.GetService<LoginService>().CleanupAsync();
        await host.GetService<MetricService>().CleanupAsync();
    }
}
