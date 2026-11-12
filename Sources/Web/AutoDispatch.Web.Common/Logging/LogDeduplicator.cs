using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace AutoDispatch.Web.Common;

/// <summary>
///     Фильтр перед БД-синком: одно и то же сообщение Warning+ (уровень, пользователь, Scope, текст без цифр) проходит не больше
///     MaxPerWindow раз за окно, остальное отбрасывается. Файл получает всё. Information не режется: запросы одного пользователя
///     законно повторяются чаще </summary>
public class LogDeduplicator : ILogEventFilter {

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    public const int MaxPerWindow = 5;
    private const int MaxKeys = 1000;

    private static readonly Regex Digits = new(@"\d+", RegexOptions.Compiled);

    private readonly Func<DateTime> _now;
    private readonly Dictionary<string, (DateTime Started, int Count)> _entries = new();

    public LogDeduplicator(Func<DateTime> now = null) {
        this._now = now ?? (() => DateTime.UtcNow);
    }

    public bool IsEnabled(LogEvent logEvent) {
        return IsEnabled(logEvent.Level, Scalar(logEvent, "UserName"), Scalar(logEvent, AppSettings.LogScopeProperty), logEvent.RenderMessage());
    }

    public bool IsEnabled(LogEventLevel level, string userName, string scope, string message) {
        if (level < LogEventLevel.Warning) {
            return true;
        }
        var key = $"{level}|{userName}|{scope}|{Digits.Replace(message, "#")}";
        var now = _now();
        lock (_entries) {
            if (_entries.TryGetValue(key, out var entry) && now - entry.Started < Window) {
                _entries[key] = (entry.Started, entry.Count + 1);
                return entry.Count < MaxPerWindow;
            }
            if (entry == default && _entries.Count >= MaxKeys) {
                Prune(now);
            }
            _entries[key] = (now, 1);
            return true;
        }
    }

    #region [ private ]

    private void Prune(DateTime now) {
        foreach (var key in _entries.Where(e => now - e.Value.Started >= Window).Select(e => e.Key).ToList()) {
            _entries.Remove(key);
        }
        if (_entries.Count >= MaxKeys) {
            _entries.Clear();
        }
    }

    private static string Scalar(LogEvent logEvent, string name) {
        return logEvent.Properties.TryGetValue(name, out var value) && value is ScalarValue scalar ? scalar.Value?.ToString() : null;
    }

    #endregion [ private ]
}
