using System;
using System.Threading;

namespace AutoCall.Common {

    /// <summary>
    ///     Id запуска App: колонка Session в логах сервера (Api/Logs) и префикс X-Request-Id каждого запроса к серверу —
    ///     по нему запись запроса на сервере находится в ленте клиента. Копия AutoCall V3 </summary>
    public static class AppSession {

        public static readonly string Id = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";

        private static int _requests;

        public static string NextRequestId() => $"{Id}-{Interlocked.Increment(ref _requests)}";
    }

    /// <summary>
    ///     Свойства Serilog, которые ServerLogSink переносит в запись сервера: Scope — источник (колонка Scope, без префикса app.),
    ///     Details — раскрываемые детали (исключение кладётся само). Копия AutoCall V3 </summary>
    public static class LogProperties {
        public const string Scope = "Scope";
        public const string Details = "Details";
    }

    /// <summary>
    ///     Значения Scope App; на сервере — app.session, app.dispatch … Логгер: Serilog.Log.ForContext(LogProperties.Scope, LogScopes.Dispatch) </summary>
    public static class LogScopes {
        public const string Session = "session";
        public const string Dispatch = "dispatch";
        public const string Sip = "sip";
        public const string Ui = "ui";
        /// <summary> V1 _webl («WebLogger») </summary>
        public const string Web = "web";
    }
}
