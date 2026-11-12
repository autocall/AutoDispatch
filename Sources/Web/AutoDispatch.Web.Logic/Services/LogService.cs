using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Log;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Dtos.Log;
using AutoDispatch.Web.Logic.Monitoring;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Чтение таблицы Log, которую пишет Serilog MSSqlServer sink, и запись логов App (Api/Logs) через _l </summary>
public class LogService : BaseService {

    private IQueryable<LogEntry> Table => UnitOfWork.LogContext.GetTable<LogEntry>();
    private ClientLogQuota Quota => Host.ServiceProvider.GetRequiredService<ClientLogQuota>();

    public LogService(ServicesHost host) : base(host) { }

    #region [ Client ]

    /// <summary>
    ///     Пачка App в лог: уровень и Scope из записи (Scope только app.*), Session и ClientTime свойствами, UserName ставит
    ///     RequestContextMiddleware. Сверх квоты — пачка отбрасывается, одно предупреждение на окно </summary>
    public void WriteClient(int userId, ClientLogBatchDto dto) {
        var entries = dto.Entries.Where(e => String.IsNullOrWhiteSpace(e.Message) == false).Take(AppSettings.ClientLogBatchLimit).ToList();
        if (entries.Count == 0) {
            return;
        }
        if (Quota.TryTake(userId, entries.Count, out var firstDrop) == false) {
            if (firstDrop) {
                _l.w($"Client log quota exceeded: {AppSettings.ClientLogQuota} entries per {AppSettings.ClientLogQuotaWindow.TotalMinutes:F0} min, batches are dropped");
            }
            return;
        }
        using var session = _l.Property(AppSettings.LogSessionProperty, dto.Session?.Truncate(32));
        foreach (var entry in entries) {
            var level = Enum.TryParse<LogEventLevel>(entry.Level, true, out var parsed) ? parsed : LogEventLevel.Information;
            using var scope = _l.Scope(ClientScope(entry.Scope));
            using var details = _l.Property(AppSettings.LogDetailsProperty, entry.Details.Truncate(AppSettings.ClientLogDetailsMaxLength));
            using var time = _l.Property(AppSettings.LogClientTimeProperty, entry.Time == default ? null : entry.Time);
            _l.Log(level, entry.Message.Trim().Truncate(AppSettings.ClientLogMessageMaxLength));
        }
    }

    /// <summary>
    ///     Scope клиента всегда под app.: «dat» и «app.dat» → app.dat, пусто → app </summary>
    public static string ClientScope(string scope) {
        scope = (scope ?? "").Trim();
        if (scope.StartsWith("app.")) {
            scope = scope.Substring(4);
        }
        return (scope.Length == 0 ? "app" : $"app.{scope}").Truncate(50);
    }

    #endregion [ Client ]

    /// <summary>
    ///     Строк и МБ таблицы Log для /Admin/Monitor; sp_spaceused не требует прав сверх public </summary>
    public async Task<LogTableSizeDto> GetTableSizeAsync() {
        var row = (await UnitOfWork.LogContext.QueryToListAsync<SpaceUsed>($"exec sp_spaceused '{AppSettings.LogTableName}'")).FirstOrDefault();
        if (row == null) {
            return null;
        }
        // rows — char(20) с пробелами, reserved — «12345 KB»; прирост — count по IX_Log_TimeStamp
        var now = DateTime.UtcNow;
        var week = await Table.Where(e => e.TimeStamp >= now.AddDays(-7))
            .GroupBy(e => e.TimeStamp >= now.AddDays(-1))
            .Select(g => new { LastDay = g.Key, Count = g.Count() })
            .ToListAsync();
        var lastDay = week.Where(e => e.LastDay).Sum(e => e.Count);
        return new LogTableSizeDto {
            Rows = Int64.Parse(row.Rows.Trim()),
            Megabytes = Int64.Parse(row.Reserved.Replace(" KB", "")) / 1024.0,
            LastDayRows = lastDay,
            DayAverageRows = week.Sum(e => e.Count) / 7
        };
    }

    private class SpaceUsed {
        [Column("rows")] public string Rows { get; set; }
        [Column("reserved")] public string Reserved { get; set; }
    }

    public async Task<List<LogEntry>> SearchAsync(LogFilterDto filter) {
        var query = Table;
        if (String.IsNullOrEmpty(filter.Level) == false) {
            var index = Array.IndexOf(LogEntry.Levels, filter.Level);
            if (index > 0) {
                var levels = LogEntry.Levels.Skip(index).ToArray();
                query = query.Where(e => levels.Contains(e.Level));
            }
        }
        if (filter.Scope.Count > 0) {
            query = query.Where(e => filter.Scope.Contains(e.Scope));
        } else if (filter.Group == "app") {
            query = query.Where(e => e.Scope == "app" || e.Scope.StartsWith("app."));
        } else if (filter.Group == "server") {
            query = query.Where(e => e.Scope == null || (e.Scope != "app" && e.Scope.StartsWith("app.") == false));
        }
        if (String.IsNullOrWhiteSpace(filter.UserName) == false) {
            var userName = filter.UserName.Trim();
            query = query.Where(e => e.UserName.Contains(userName));
        }
        if (String.IsNullOrEmpty(filter.Text) == false) {
            query = query.Where(e => e.Message.Contains(filter.Text) || e.Details.Contains(filter.Text));
        }
        if (String.IsNullOrWhiteSpace(filter.Session) == false) {
            var session = filter.Session.Trim();
            query = query.Where(e => e.Session == session);
        }
        if (filter.Requests == false) {
            query = query.Where(e => e.SourceContext == null || e.SourceContext != RequestLoggingSource);
        }
        var now = DateTime.UtcNow;
        if (filter.EffectiveFrom(now) is { } from) {
            query = query.Where(e => e.TimeStamp >= from);
        }
        if (filter.EffectiveTo() is { } to) {
            query = query.Where(e => e.TimeStamp < to);
        }
        if (filter.Before != null) {
            query = query.Where(e => e.Id < filter.Before);
        }
        return await SafeAsync(() => query.OrderByDescending(e => e.Id).Take(AppSettings.LogPageSize).ToListAsync(), new List<LogEntry>());
    }

    /// <summary>
    ///     SourceContext записей UseSerilogRequestLogging — по умолчанию скрыты на странице </summary>
    public const string RequestLoggingSource = "Serilog.AspNetCore.RequestLoggingMiddleware";

    /// <summary>
    ///     Значения Scope за последние 7 дней — для фильтра </summary>
    public async Task<List<string>> GetScopesAsync() {
        var since = DateTime.UtcNow.AddDays(-7);
        return await SafeAsync(() => Table.Where(e => e.TimeStamp >= since && e.Scope != null).Select(e => e.Scope).Distinct().OrderBy(e => e).ToListAsync(), new List<string>());
    }

    public async Task<LogStatsDto> GetStatsAsync() {
        var now = DateTime.UtcNow;
        var day = await SafeAsync(() => Table.Where(e => e.TimeStamp >= now.AddDays(-1))
            .GroupBy(e => new { e.Level, LastHour = e.TimeStamp >= now.AddHours(-1) })
            .Select(g => new { g.Key.Level, g.Key.LastHour, Count = g.Count() })
            .ToListAsync(), []);
        var stats = new LogStatsDto();
        foreach (var row in day) {
            stats.LastDay[row.Level] = stats.LastDay.GetValueOrDefault(row.Level) + row.Count;
            if (row.LastHour) {
                stats.LastHour[row.Level] = stats.LastHour.GetValueOrDefault(row.Level) + row.Count;
            }
        }
        return stats;
    }

    #region [ private ]

    /// <summary>
    ///     Таблицу Log создаёт Serilog sink при первой инициализации; до неё страницы отдают пустой результат, не ошибку </summary>
    private static async Task<T> SafeAsync<T>(Func<Task<T>> query, T fallback) {
        try {
            return await query();
        } catch (Microsoft.Data.SqlClient.SqlException e) when (e.Number == 208) {
            _l.w($"Log table is missing: {e.Message}");
            return fallback;
        }
    }

    #endregion [ private ]
}
