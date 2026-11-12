using Serilog;
using Serilog.Events;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Common;

/// <summary>
///     Обёртка над Serilog: сообщение пишется как есть, не как шаблон ({} в тексте не интерпретируются) </summary>
public class CommonLogger : ICommonLogger {

    private readonly ILogger _logger;

    public CommonLogger(ILogger logger) {
        this._logger = logger;
    }

    public void Log(LogEventLevel level, string message) {
        _logger.Write(level, "{Message:l}", message.Truncate(4000));
    }

    public void Log(LogEventLevel level, string message, Exception ex) {
        _logger.Write(level, ex, "{Message:l}", message.Truncate(4000));
    }
}
