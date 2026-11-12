using System.ComponentModel;

namespace AutoDispatch.Web.Data.Enums;

/// <summary>
///     Метрика в таблице Metric (БД логов): одно значение за минуту. Серверные — 1..29, клиентские (отчёт App) — с 30.
///     Как агрегируются значения минуты — MetricConverter.TypeToKind. Метрики dispatch (long-polling, Request, внешние сервисы) — этап 8 плана </summary>
public enum MetricTypeEnum : byte {
    Unknown = 0,

    [Description("Web CPU")]
    WebCpu = 1,
    [Description("DB CPU")]
    DbCpu = 2,
    [Description("Geocoding")]
    GeoLatency = 3,
    [Description("Geocoding")]
    GeoCounter = 4,
    [Description("Geocoding")]
    GeoErrorCounter = 5,

    /// <summary> App открыт: от пользователя за минуту пришёл отчёт </summary>
    [Description("Online")]
    OnlineUsers = 30,

    [Description("App exceptions")]
    AppExceptions = 47
}

/// <summary>
///     Как значения одной минуты сводятся в одно </summary>
public enum MetricKindEnum : byte {
    Unknown = 0,
    /// <summary> Медиана замеров (CPU, задержка) </summary>
    Median = 1,
    /// <summary> Сумма за минуту (счётчики: отчёты клиента покрывают непересекающиеся 30 с) </summary>
    Sum = 2,
    /// <summary> Различные пользователи, у которых признак был хотя бы в одном отчёте минуты: два отчёта не удваивают </summary>
    DistinctUsers = 3,
    /// <summary> Различные пользователи, у которых признак есть в последнем отчёте минуты (состояние, а не событие) </summary>
    LastPerUser = 4
}
