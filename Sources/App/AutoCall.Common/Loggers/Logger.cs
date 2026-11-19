using System;
using System.Diagnostics;
using AutoCall.Common.Extensions;
using Serilog.Events;

namespace AutoCall.Common {

    /// <summary>
    ///     Logging class </summary>
    /// <remarks>
    /// Info - logs of user actions or outside events
    /// Debug - logs of detail debuggin. default was turned off
    /// Warning - logs of notes
    /// Error - logs of occur errors
    /// Fatal - logs of crush application
    ///
    /// V1 писал через Common.Logging/log4net; в AutoDispatch App — Serilog, как AutoCall V3 (файл + ServerLogSink, App.xaml.cs).
    /// API (_l.i/w/e/f/d, di/dd/dt/it) прежний </remarks>
    public class _l {

        private static bool _debug = false;
        private static bool _stop = false;

        public static void Stop() {
            _stop = true;
        }

        #region [ Static ]

        public static string EscapeBrace(string message) {
            return message.Replace("{", "{{").Replace("}", "}}");
        }

        public static void ShowDebug() {
            _debug = true;
        }

        public static void SkipDebug() {
            _debug = false;
        }

        #endregion [ Static ]

        #region [ Internal ]

        /// <summary>
        ///     Текст как в V1: string.Format по args, без args — как есть; не web-логи режутся до 5000 символов </summary>
        private static string Format(string message, object[] args, bool truncate) {
            if (args != null && args.Length > 0) {
                try {
                    message = String.Format(message, args);
                } catch (FormatException) {
                }
            }
            return truncate ? message.Truncate(5000) : message;
        }

        internal static void _log(Serilog.ILogger logger, LogEventLevel level, string message, Exception ex, bool truncate, params object[] args) {
            if (_stop) return;
            var text = Format(message, args, truncate);
            // как V1: InfoException (ожидаемая ошибка для пользователя) в лог не пишется
            if (ex == null || ex.GetType().Name != "InfoException") {
                logger.Write(level, ex, "{Message:l}", text);
            }
            if (ex != null) {
                Debug.WriteLine(text + "\n" + ex.GetBaseException().ToString());
            } else {
                Debug.WriteLine(text);
            }
        }

        #endregion [ Internal ]

        #region [ Public ]

        internal static void Log(LogEventLevel level, string message, params object[] args) {
            _log(Serilog.Log.Logger, level, message, null, true, args);
        }

        internal static void Log(LogEventLevel level, string message, Exception ex, params object[] args) {
            _log(Serilog.Log.Logger, level, message, ex, true, args);
        }

        /// <summary>
        ///     Write log from clients </summary>
        public static void Log(string message) {
            Log(LogEventLevel.Information, message);
        }

        public static void i(string message, params object[] args) {
            Log(LogEventLevel.Information, message, args);
        }

        public static void w(string message, params object[] args) {
            Log(LogEventLevel.Warning, message, args);
        }
        public static void w(string message, Exception ex, params object[] args) {
            Log(LogEventLevel.Warning, message, ex, args);
        }

        public static void e(string message, params object[] args) {
            Log(LogEventLevel.Error, message, args);
        }
        public static void e(string message, Exception ex, params object[] args) {
            Log(LogEventLevel.Error, message, ex, args);
        }

        public static void f(string message, params object[] args) {
            Log(LogEventLevel.Fatal, message, args);
        }
        public static void f(string message, Exception ex, params object[] args) {
            Log(LogEventLevel.Fatal, message, ex, args);
        }

        public static void d(string message, params object[] args) {
            Log(LogEventLevel.Debug, message, args);
        }
        public static void d(string message, Exception ex, params object[] args) {
            Log(LogEventLevel.Debug, message, ex, args);
        }

        /// <summary>
        ///     Logging duration of action as Info log</summary>
        public static LoggerItem di(string message, params object[] args) {
            var loggerItem = new LoggerItem((delta, log) => i($"{message}\nDuration: {delta.TotalSeconds:0.000}sec.{log}", args));
            return loggerItem;
        }
        /// <summary>
        ///     Logging duration of action as Debug log</summary>
        public static LoggerItem dd(string message, params object[] args) {
            var loggerItem = new LoggerItem((delta, log) => d($"{message}\nDuration: {delta.TotalSeconds:0.000}sec.{log}", args));
            return loggerItem;
        }

        public static IDisposable dt(string message) {
            return new LogItem((ts) => d($"{message} {StringExtension.ToString(ts)}"));
        }

        public static IDisposable it(string message) {
            return new LogItem((ts) => i($"{message} {StringExtension.ToString(ts)}"));
        }

        #endregion [ Public ]
    }

    public class LogItem : IDisposable {
        private Action<TimeSpan> _action;
        private DateTime _started = DateTime.UtcNow;

        public LogItem(Action<TimeSpan> action) {
            this._action = action;
        }

        public void Dispose() {
            this._action(DateTime.UtcNow - this._started);
        }
    }

    public sealed class LoggerItem : IDisposable {

        /// <summary>
        ///     Added log </summary>
        private string _addLog { get; set; }

        /// <summary>
        ///     Action of logging </summary>
        private Action<TimeSpan, string> _action;

        /// <summary>
        ///     Start of action </summary>
        private DateTime _start;

        /// <summary>
        ///     Disposed flag </summary>
        private bool _disposed;

        /// <summary>
        ///     .ctor </summary>
        public LoggerItem(Action<TimeSpan, string> action) {
            _start = DateTime.UtcNow;
            _action = action;
        }

        /// <summary>
        ///     Add message log to end </summary>
        public void log(string message, params object[] args) {
            _addLog += String.Format("\n" + message, args);
        }

        /// <summary>
        ///     Cancel log </summary>
        public void cancel() {
            _disposed = true;
        }

        /// <summary>
        ///     Dispose </summary>
        public void Dispose() {
            if (_disposed == false) {
                _disposed = true;
                _action.Invoke(DateTime.UtcNow - _start, _addLog);
            }
        }
    }
}