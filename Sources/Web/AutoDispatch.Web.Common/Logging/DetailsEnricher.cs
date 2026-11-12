using Serilog.Core;
using Serilog.Events;

namespace AutoDispatch.Web.Common;

/// <summary>
///     Исключение записи → свойство Details (колонка таблицы Log) в коротком виде ExceptionFormatter; колонки Exception в таблице нет.
///     Details, положенный через LogContext, исключение перекрывает </summary>
public class DetailsEnricher : ILogEventEnricher {

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        if (logEvent.Exception != null) {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(AppSettings.LogDetailsProperty, ExceptionFormatter.Format(logEvent.Exception)));
        }
    }
}
