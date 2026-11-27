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
///     Не накапливается: неотправленный отчёт теряется. Показатели dispatch — этап 8 плана; телефония — как AutoCall V3 </summary>
public class ClientMetricsDto {

    /// <summary>
    ///     По телефонии: MicroSIP, AbtoPhone (CallServiceType App) </summary>
    public List<ClientCallMetricsDto> Calls { get; set; } = [];

    /// <summary>
    ///     Регистрация софтфона на PBX; null — не известна (телефония не запущена или не сообщает) </summary>
    public bool? SipRegistered { get; set; }

    /// <summary>
    ///     Необработанные исключения App за 30 с </summary>
    public int Exceptions { get; set; }

    /// <summary>
    ///     Долгие запросы App к серверу (старый App поле не шлёт — 0) </summary>
    public int SlowRequests { get; set; }
}

public class ClientCallMetricsDto {
    public string Service { get; set; }
    /// <summary>
    ///     Дозвонился или не ответили </summary>
    public int Calls { get; set; }
    public int Errors { get; set; }
}
