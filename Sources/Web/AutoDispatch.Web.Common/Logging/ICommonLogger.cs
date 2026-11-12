using Serilog.Events;

namespace AutoDispatch.Web.Common;

public interface ICommonLogger {
    void Log(LogEventLevel level, string message);
    void Log(LogEventLevel level, string message, Exception ex);
}
