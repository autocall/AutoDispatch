using Serilog.Context;
using Serilog.Events;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Common.Extensions;

namespace AutoDispatch.Web.Common;

/// <summary>
///     Фасад логирования: _l.i/w/e/d/f, замер времени через using (_l.it("...")),
///     область через using (_l.Scope("auth")) — все записи внутри получают свойство Scope=auth </summary>
public static class _l {

    #region [ .ctor ]

    private static ICommonLogger _instance;

    public static void Init(ICommonLogger instance) {
        _instance = instance;
    }

    #endregion [ .ctor ]

    #region [ Public ]

    public static void i(string message) {
        _instance?.Log(LogEventLevel.Information, message);
    }

    public static void w(string message) {
        _instance?.Log(LogEventLevel.Warning, message);
    }
    public static void w(string message, Exception ex) {
        if (ex is InfoException == false) {
            _instance?.Log(LogEventLevel.Warning, message, ex);
        }
    }

    public static void e(string message) {
        _instance?.Log(LogEventLevel.Error, message);
    }
    public static void e(string message, Exception ex) {
        if (ex is InfoException == false) {
            _instance?.Log(LogEventLevel.Error, message, ex);
        }
    }

    public static void f(string message) {
        _instance?.Log(LogEventLevel.Fatal, message);
    }
    public static void f(string message, Exception ex) {
        _instance?.Log(LogEventLevel.Fatal, message, ex);
    }

    public static void d(string message) {
        _instance?.Log(LogEventLevel.Debug, message);
    }
    public static void d(object obj) {
        _instance?.Log(LogEventLevel.Debug, JsonExtension.Serialize(obj));
    }

    /// <summary>
    ///     Уровень известен только в runtime (записи App из Api/Logs) </summary>
    public static void Log(LogEventLevel level, string message) {
        _instance?.Log(level, message);
    }

    /// <summary>
    ///     Помечает все записи внутри using свойством Scope (AppSettings.LogScopeProperty) для фильтрации </summary>
    public static IDisposable Scope(string name) {
        return LogContext.PushProperty(AppSettings.LogScopeProperty, name);
    }

    /// <summary>
    ///     Любое свойство на записи внутри using: Details, Session, ClientTime </summary>
    public static IDisposable Property(string name, object value) {
        return LogContext.PushProperty(name, value);
    }

    /// <summary>
    ///     Замер времени: пишет message и длительность при Dispose, уровень Information </summary>
    public static IDisposable it(string message) {
        return new LogItem((ts) => i($"{message} {StringExtension.ToString(ts)}"));
    }

    /// <summary>
    ///     Замер времени, уровень Debug </summary>
    public static IDisposable dt(string message) {
        return new LogItem((ts) => d($"{message} {StringExtension.ToString(ts)}"));
    }

    #endregion [ Public ]
}

public class LogItem : IDisposable {

    private readonly Action<TimeSpan> _action;
    private readonly DateTime _started = DateTime.UtcNow;

    public LogItem(Action<TimeSpan> action) {
        this._action = action;
    }

    public void Dispose() {
        this._action(DateTime.UtcNow - this._started);
    }
}
