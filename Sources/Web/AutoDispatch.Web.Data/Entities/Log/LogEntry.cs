namespace AutoDispatch.Web.Data.Entities.Log;

/// <summary>
///     Строка таблицы Log, которую создаёт и пишет Serilog MSSqlServer sink. Только чтение через LinqToDB, в EF-модель не входит;
///     колонки Details и Session добавлены миграцией AdLogDbContext </summary>
public class LogEntry {
    public int Id { get; set; }
    /// <summary>
    ///     Короткий заголовок записи </summary>
    public string Message { get; set; }
    /// <summary>
    ///     Verbose, Debug, Information, Warning, Error, Fatal </summary>
    public string Level { get; set; }
    public DateTime TimeStamp { get; set; }
    /// <summary>
    ///     Остальные свойства события, JSON </summary>
    public string LogEvent { get; set; }
    public string Scope { get; set; }
    public string UserName { get; set; }
    public string RequestPath { get; set; }
    public string SourceContext { get; set; }
    /// <summary>
    ///     Раскрываемые детали: короткий текст исключения (ExceptionFormatter), JSON паттерна и т.п. </summary>
    public string Details { get; set; }
    /// <summary>
    ///     Id запуска App, для записей из Api/Logs </summary>
    public string Session { get; set; }

    public static readonly string[] Levels = ["Verbose", "Debug", "Information", "Warning", "Error", "Fatal"];
}
