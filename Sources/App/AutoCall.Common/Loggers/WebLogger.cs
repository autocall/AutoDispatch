using System;
using Serilog.Events;

namespace AutoCall.Common {
    /// <summary>
    ///     V1: отдельный log4net-логгер «WebLogger» (ошибки для сервера), не чаще раза в минуту и без повтора подряд.
    ///     В AutoDispatch App — тот же Serilog со Scope = web: на сервер Information+ уходят через ServerLogSink, как в AutoCall V3.
    ///     Ограничение V1 «раз в минуту» снято (2026-11-27, AI/wiki/plan.md): оно стояло до Serilog и теряло ошибки и в локальном
    ///     файле — за 10 минут без сервера оставалось ~10 строк. Повторы режет ServerLogSink (5 одинаковых в минуту + итог) </summary>
    public class _webl {
        private static Serilog.ILogger logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Web);

        internal static void Log(LogEventLevel level, string message, params object[] args) {
            Log(level, message, null, args);
        }

        internal static void Log(LogEventLevel level, string message, Exception ex, params object[] args) {
            _l._log(logger, level, message, ex, false, args);
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

    }
}
