using System;
using AutoCall.Common.Extensions;
using Serilog.Events;

namespace AutoCall.Common {
    /// <summary>
    ///     V1: отдельный log4net-логгер «WebLogger» (ошибки для сервера), не чаще раза в минуту и без повтора подряд.
    ///     В AutoDispatch App — тот же Serilog со Scope = web: на сервер Information+ уходят через ServerLogSink, как в AutoCall V3 </summary>
    public class _webl {
        private static Serilog.ILogger logger => Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Web);

        private static DateTime LastLog = DateTime.MinValue;
        private static TimeSpan LogDelta = TimeSpan.FromMinutes(1);

        private static int _lastMessageHash = 0;

        internal static void Log(LogEventLevel level, string message, params object[] args) {
            Log(level, message, null, args);
        }

        internal static void Log(LogEventLevel level, string message, Exception ex, params object[] args) {
            var hash = level.GetHashCode() + message.GetDeterministicHashCode();
            if (_lastMessageHash == hash) {
                return;
            }
            _lastMessageHash = hash;

            if (DateTime.UtcNow - LastLog > LogDelta) {
                _l._log(logger, level, message, ex, false, args);
                LastLog = DateTime.UtcNow;
            }
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
