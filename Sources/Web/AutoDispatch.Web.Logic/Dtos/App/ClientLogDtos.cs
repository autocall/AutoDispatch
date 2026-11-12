namespace AutoDispatch.Web.Logic.Dtos.App;

/// <summary>
///     Пачка логов App (Api/Logs): записи Information+ за 15–30 с, пользователь — из токена, Session — id запуска App </summary>
public class ClientLogBatchDto {
    public string Session { get; set; }
    public List<ClientLogEntryDto> Entries { get; set; } = [];
}

public class ClientLogEntryDto {
    /// <summary>
    ///     Клиентское время, UTC </summary>
    public DateTime Time { get; set; }
    /// <summary>
    ///     Уровень Serilog: Information, Warning, Error, Fatal </summary>
    public string Level { get; set; }
    /// <summary>
    ///     Источник без префикса app.: session, dat, datsoap, truckstop, sip, pattern, ui </summary>
    public string Scope { get; set; }
    /// <summary>
    ///     Короткий заголовок </summary>
    public string Message { get; set; }
    /// <summary>
    ///     Раскрываемые детали: JSON паттерна, текст исключения </summary>
    public string Details { get; set; }
}
