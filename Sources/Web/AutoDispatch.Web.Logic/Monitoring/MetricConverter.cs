using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Как агрегируется и как оценивается метрика (как MetricConverter в Safer) </summary>
public static class MetricConverter {

    public static MetricKindEnum TypeToKind(MetricTypeEnum type) {
        switch (type) {
            case MetricTypeEnum.WebCpu:
            case MetricTypeEnum.DbCpu:
            case MetricTypeEnum.GeoLatency:
                return MetricKindEnum.Median;
            case MetricTypeEnum.OnlineUsers:
                return MetricKindEnum.DistinctUsers;
            default:
                return MetricKindEnum.Sum;
        }
    }

    /// <summary>
    ///     Границы ступеней по возрастанию: предупреждение, ошибка; null — у метрики шкалы нет.
    ///     CPU, %; задержка geocoding, мс </summary>
    public static int[] GetSteps(MetricTypeEnum type) {
        switch (type) {
            case MetricTypeEnum.WebCpu:
            case MetricTypeEnum.DbCpu:
                return [70, 90];
            case MetricTypeEnum.GeoLatency:
                return [1500, 3000];
            default:
                return null;
        }
    }

    /// <summary>
    ///     Ступень значения: 0 — норма, 1 — предупреждение, 2 — ошибка; null — нет данных или у метрики нет шкалы </summary>
    public static int? GetStep(MetricTypeEnum type, int? value) {
        if (value == null || GetSteps(type) is not { } steps) {
            return null;
        }
        var step = 0;
        while (step < steps.Length && value >= steps[step]) {
            step++;
        }
        return step;
    }
}
