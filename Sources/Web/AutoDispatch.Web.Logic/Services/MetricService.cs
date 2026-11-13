using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Entities.Log;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Dtos.App;
using AutoDispatch.Web.Logic.Monitoring;

namespace AutoDispatch.Web.Logic.Services;

/// <summary>
///     Метрики (как Safer): Web CPU, DB CPU от скрипта на сервере БД, сохранение закрытых минут в Metric (БД логов), чтение для /Admin/Metrics </summary>
public class MetricService : BaseService {

    private MetricCollector Collector => Host.ServiceProvider.GetRequiredService<MetricCollector>();

    public MetricService(ServicesHost host) : base(host) { }

    public void AddClientReport(Guid userId, ClientMetricsDto dto) {
        Collector.AddClientReport(userId, dto);
    }

    /// <summary>
    ///     Значение от внешнего скрипта (Api/Metrics/Add): DB CPU — сервер БД шлёт его сам, SQL-ом с другого сервера не снять </summary>
    public void Add(MetricTypeEnum type, int value) {
        Collector.Add(type, value);
    }

    /// <summary>
    ///     Замер CPU веб-сервера (Linux, /proc/stat) </summary>
    public async Task MeasureCpuAsync(CancellationToken cancellation) {
        if (await GetWebCpuAsync(cancellation) is { } web) {
            Collector.Add(MetricTypeEnum.WebCpu, web);
        }
    }

    /// <summary>
    ///     Закрытые минуты — в таблицу Metric </summary>
    public async Task SaveAsync() {
        var rows = Collector.TakeClosed();
        if (rows.Count > 0) {
            await UnitOfWork.LogContext.BulkCopyAsync(rows);
        }
    }

    /// <summary>
    ///     Строки таблицы по возрастанию времени (новые внизу).
    ///     Minutes: минуты часа hourUtc, без него — последние MetricRecentPeriods.
    ///     Hours: часы локального дня day, без него — последние MetricRecentPeriods.
    ///     Days: последние MetricRecentPeriods локальных дней (сервера) </summary>
    public async Task<List<MetricRowDto>> GetRowsAsync(MetricLevelEnum level, DateTime? day, DateTime? hourUtc) {
        var now = DateTime.UtcNow;
        var periods = new List<(DateTime Start, DateTime End)>();
        switch (level) {
            case MetricLevelEnum.Days: {
                var today = DateTime.Now.Date;
                for (var date = today.AddDays(-(AppSettings.MetricRecentPeriods - 1)); date <= today; date = date.AddDays(1)) {
                    periods.Add((ToUtc(date), ToUtc(date.AddDays(1))));
                }
                break;
            }
            case MetricLevelEnum.Hours: {
                var from = day != null ? ToUtc(day.Value.Date) : RoundHour(now).AddHours(-(AppSettings.MetricRecentPeriods - 1));
                for (var hour = 0; hour < (day != null ? 24 : AppSettings.MetricRecentPeriods); hour++) {
                    periods.Add((from.AddHours(hour), from.AddHours(hour + 1)));
                }
                break;
            }
            default: {
                var current = MetricCollector.RoundMinute(now);
                var from = hourUtc ?? current.AddMinutes(-AppSettings.MetricRecentPeriods);
                var to = hourUtc?.AddHours(1) ?? current;
                // текущая минута ещё в памяти MetricCollector, в таблице её нет
                for (var minute = from; minute < to && minute < current; minute = minute.AddMinutes(1)) {
                    periods.Add((minute, minute.AddMinutes(1)));
                }
                break;
            }
        }
        if (periods.Count == 0) {
            return [];
        }

        // минуты — как есть; часы и дни — из часовых агрегатов (сумма, максимум, число минут), день — из часов
        var buckets = level == MetricLevelEnum.Minutes
            ? await this.GetMinuteBucketsAsync(periods[0].Start, periods[^1].End)
            : await this.GetHourBucketsAsync(periods[0].Start, periods[^1].End);
        return periods.Select(period => ToRow(period.Start, buckets.Where(e => e.Start >= period.Start && e.Start < period.End)))
            .ToList();
    }


    public async Task CleanupAsync() {
        var deleted = await UnitOfWork.LogContext.GetTable<Metric>()
            .Where(e => e.DateTime < DateTime.UtcNow.AddDays(-AppSettings.MetricRetainedDays)).DeleteAsync();
        if (deleted > 0) {
            _l.i($"Retention: Metric {deleted} rows deleted");
        }
    }

    #region [ private ]

    /// <summary>
    ///     Значения метрики за минуту или час: сумма, максимум и число минут — из них сворачиваются любые периоды </summary>
    private record Bucket(DateTime Start, MetricTypeEnum Type, long Sum, int Max, int Count);

    private async Task<List<Bucket>> GetMinuteBucketsAsync(DateTime from, DateTime to) {
        var rows = await UnitOfWork.LogContext.GetTable<Metric>().Where(e => e.DateTime >= from && e.DateTime < to).ToListAsync();
        return rows.Select(e => new Bucket(DateTime.SpecifyKind(e.DateTime, DateTimeKind.Utc), e.MetricId, e.Value, e.Value, 1)).ToList();
    }

    private async Task<List<Bucket>> GetHourBucketsAsync(DateTime from, DateTime to) {
        var rows = await UnitOfWork.LogContext.GetTable<Metric>()
            .Where(e => e.DateTime >= from && e.DateTime < to)
            .GroupBy(e => new { e.DateTime.Year, e.DateTime.Month, e.DateTime.Day, e.DateTime.Hour, e.MetricId })
            .Select(g => new {
                g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, g.Key.MetricId,
                Sum = g.Sum(e => (long)e.Value), Max = g.Max(e => e.Value), Count = g.Count()
            })
            .ToListAsync();
        return rows.Select(e => new Bucket(new DateTime(e.Year, e.Month, e.Day, e.Hour, 0, 0, DateTimeKind.Utc), e.MetricId, e.Sum, e.Max, e.Count))
            .ToList();
    }

    /// <summary>
    ///     Сворачивание периода: события — сумма, CPU и задержка — среднее (цвет по максимуму), пользователи — пик </summary>
    private static MetricRowDto ToRow(DateTime start, IEnumerable<Bucket> buckets) {
        var row = new MetricRowDto { Start = start };
        foreach (var group in buckets.GroupBy(e => e.Type)) {
            var sum = group.Sum(e => e.Sum);
            var max = group.Max(e => e.Max);
            var count = group.Sum(e => e.Count);
            switch (MetricConverter.TypeToKind(group.Key)) {
                case MetricKindEnum.Median:
                    row.Values[group.Key] = (int)(sum / count);
                    row.StatusValues[group.Key] = max;
                    break;
                case MetricKindEnum.Sum:
                    row.Values[group.Key] = row.StatusValues[group.Key] = (int)Math.Min(sum, Int32.MaxValue);
                    break;
                default:
                    row.Values[group.Key] = row.StatusValues[group.Key] = max;
                    break;
            }
        }
        return row;
    }

    private static DateTime RoundHour(DateTime utc) => new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);

    /// <summary>
    ///     Локальное время сервера (границы дней и часов на странице) → UTC </summary>
    private static DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZoneInfo.Local);

    /// <summary>
    ///     Как MerticHelper.GetCpuValueAsync в Safer: два снимка /proc/stat с интервалом в секунду. В контейнере это CPU хоста </summary>
    private static async Task<int?> GetWebCpuAsync(CancellationToken cancellation) {
        if (OperatingSystem.IsLinux() == false) {
            return null;
        }
        static (long Idle, long Total) Read() {
            var stats = File.ReadLines("/proc/stat").First().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return (Int64.Parse(stats[4]), stats.Skip(1).Select(Int64.Parse).Sum());
        }
        var first = Read();
        await Task.Delay(TimeSpan.FromSeconds(1), cancellation);
        var second = Read();
        var total = second.Total - first.Total;
        return total > 0 ? (int)(100.0 * (total - (second.Idle - first.Idle)) / total) : null;
    }

    #endregion [ private ]
}
