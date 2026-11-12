using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Dtos.App;

/// <summary>
///     Одно значение серверной метрики от внешнего скрипта (Api/Metrics/Add): DB CPU с сервера БД </summary>
public class ServerMetricDto {
    public MetricTypeEnum Type { get; set; }
    public int Value { get; set; }
}

/// <summary>
///     Отчёт App за последние 30 с (Api/Metrics). Сам факт отчёта — пользователь онлайн.
///     Не накапливается: неотправленный отчёт теряется. Показатели dispatch — этап 8 плана </summary>
public class ClientMetricsDto {

    /// <summary>
    ///     Необработанные исключения App за 30 с </summary>
    public int Exceptions { get; set; }
}
