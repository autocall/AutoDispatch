using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Как агрегируется и как оценивается метрика (как MetricConverter в Safer) </summary>
public static class MetricConverter {

    /// <summary>
    ///     Внешние сервисы для страницы метрик: задержка, успешные вызовы, ошибки (геокодинг Brokersnapshot — колонкой Geocoding в Server) </summary>
    public static readonly (string Name, MetricTypeEnum Latency, MetricTypeEnum Calls, MetricTypeEnum Errors)[] Externals = [
        ("CarrierView", MetricTypeEnum.CarrierViewLatency, MetricTypeEnum.CarrierViewCounter, MetricTypeEnum.CarrierViewErrorCounter),
        ("SMS", MetricTypeEnum.SmsLatency, MetricTypeEnum.SmsCounter, MetricTypeEnum.SmsErrorCounter),
        ("SMTP", MetricTypeEnum.SmtpLatency, MetricTypeEnum.SmtpCounter, MetricTypeEnum.SmtpErrorCounter),
        ("OpenAI", MetricTypeEnum.OpenAiLatency, MetricTypeEnum.OpenAiCounter, MetricTypeEnum.OpenAiErrorCounter),
        ("Companies", MetricTypeEnum.CompanyLatency, MetricTypeEnum.CompanyCounter, MetricTypeEnum.CompanyErrorCounter),
        ("Keeppingin", MetricTypeEnum.TimeZoneLatency, MetricTypeEnum.TimeZoneCounter, MetricTypeEnum.TimeZoneErrorCounter),
        ("Google Distance", MetricTypeEnum.DistanceLatency, MetricTypeEnum.DistanceCounter, MetricTypeEnum.DistanceErrorCounter)
    ];

    public static MetricKindEnum TypeToKind(MetricTypeEnum type) {
        if (Externals.Any(e => e.Latency == type)) {
            return MetricKindEnum.Median;
        }
        switch (type) {
            case MetricTypeEnum.WebCpu:
            case MetricTypeEnum.DbCpu:
            case MetricTypeEnum.GeoLatency:
            case MetricTypeEnum.DispatchRequestLatency:
            case MetricTypeEnum.DispatchPollUsers:
            case MetricTypeEnum.WebMemory:
            case MetricTypeEnum.WebDiskUsedMb:
                return MetricKindEnum.Median;
            case MetricTypeEnum.OnlineUsers:
            case MetricTypeEnum.ActiveUsers:
            case MetricTypeEnum.TrackedLoads:
                return MetricKindEnum.DistinctUsers;
            case MetricTypeEnum.AbtoPhoneUnregisteredUsers:
                return MetricKindEnum.LastPerUser;
            default:
                return MetricKindEnum.Sum;
        }
    }

    /// <summary>
    ///     Границы ступеней по возрастанию: предупреждение, ошибка; null — у метрики шкалы нет.
    ///     CPU, %; задержка, мс; память процесса, МБ. Диск — в МБ без шкалы, ступень по проценту — DiskSteps </summary>
    public static int[] GetSteps(MetricTypeEnum type) {
        if (Externals.Any(e => e.Latency == type)) {
            return [3000, 10000];
        }
        switch (type) {
            case MetricTypeEnum.WebCpu:
            case MetricTypeEnum.DbCpu:
                return [70, 90];
            case MetricTypeEnum.GeoLatency:
                return [1500, 3000];
            case MetricTypeEnum.DispatchRequestLatency:
                return [1000, 3000];
            case MetricTypeEnum.WebMemory:
                return [1500, 3000];
            default:
                return null;
        }
    }

    /// <summary>
    ///     Занятость раздела с документами, %: предупреждение, ошибка (/Admin/Monitor) </summary>
    public static readonly int[] DiskSteps = [85, 95];

    /// <summary>
    ///     Ступень занятости диска: 0 — норма, 1 — предупреждение, 2 — ошибка </summary>
    public static int DiskStep(int percent) => DiskSteps.Count(e => percent >= e);

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

    /// <summary>
    ///     Телефония клиента (CallServiceType App) → метрики звонков (как AutoCall V3) </summary>
    public static (MetricTypeEnum Calls, MetricTypeEnum Errors)? CallTypes(string service) {
        switch (service) {
            case "MicroSIP":
                return (MetricTypeEnum.MicroSipCallCounter, MetricTypeEnum.MicroSipCallErrorCounter);
            case "AbtoPhone":
                return (MetricTypeEnum.AbtoPhoneCallCounter, MetricTypeEnum.AbtoPhoneCallErrorCounter);
            default:
                return null;
        }
    }
}
