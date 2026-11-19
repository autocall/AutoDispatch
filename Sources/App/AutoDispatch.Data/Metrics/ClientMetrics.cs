#nullable enable
namespace AutoDispatch.Data.Metrics;

/// <summary>
///     Счётчики App за 30 с для Api/Metrics, как в AutoCall V3; сервер AutoDispatch принимает только Exceptions (ClientMetricsDto),
///     сорсов DAT и телефонии в отчёте нет. MetricsReporter забирает и обнуляет (TakeReport) </summary>
public static class ClientMetrics {

    private static int _exceptions;

    public static void Exception() => Interlocked.Increment(ref _exceptions);

    public static ClientMetricsReport TakeReport() {
        return new ClientMetricsReport {
            Exceptions = Interlocked.Exchange(ref _exceptions, 0),
        };
    }
}

/// <summary> Тело Api/Metrics (ClientMetricsDto сервера). Сам отчёт означает «App открыт». </summary>
public sealed class ClientMetricsReport {
    public int Exceptions { get; set; }
}
