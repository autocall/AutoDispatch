using AutoDispatch.Web.Data.Entities.Log;
using AutoDispatch.Web.Data.Enums;
using AutoDispatch.Web.Logic.Dtos.App;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Singleton: замеры по минутам в памяти, как MetricService в Safer. Закрытые минуты забирает MetricService.SaveAsync.
///     Минута — по времени прихода на сервер: отчёты App раз в 30 с, их в минуте обычно два </summary>
public class MetricCollector {

    private class Bucket {
        public readonly Dictionary<MetricTypeEnum, List<int>> Values = new();
        /// <summary>
        ///     DistinctUsers и LastPerUser: userId → признак </summary>
        public readonly Dictionary<MetricTypeEnum, Dictionary<int, bool>> Users = new();
    }

    private readonly Dictionary<DateTime, Bucket> _buckets = new();
    private readonly Func<DateTime> _utcNow;

    public MetricCollector() : this(() => DateTime.UtcNow) { }

    /// <summary>
    ///     utcNow — часы для тестов </summary>
    public MetricCollector(Func<DateTime> utcNow) {
        this._utcNow = utcNow;
    }
    private readonly object _lock = new();

    public static DateTime RoundMinute(DateTime utc) => new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);

    public void Add(MetricTypeEnum type, int value) {
        lock (_lock) {
            var values = this.Current().Values;
            if (values.TryGetValue(type, out var list) == false) {
                values[type] = list = new List<int>();
            }
            list.Add(value);
        }
    }

    /// <summary>
    ///     Вызов внешнего сервиса: задержка в мс и счётчик вызовов или ошибок </summary>
    public void AddCall(MetricTypeEnum latencyType, TimeSpan elapsed, MetricTypeEnum counterType) {
        this.Add(latencyType, (int)elapsed.TotalMilliseconds);
        this.Add(counterType, 1);
    }

    public void AddUser(MetricTypeEnum type, int userId, bool flag) {
        lock (_lock) {
            var users = this.Current().Users;
            if (users.TryGetValue(type, out var map) == false) {
                users[type] = map = new Dictionary<int, bool>();
            }
            map[userId] = MetricConverter.TypeToKind(type) == MetricKindEnum.LastPerUser
                ? flag
                : map.GetValueOrDefault(userId) || flag;
        }
    }

    public void AddClientReport(int userId, ClientMetricsDto dto) {
        // отчёт приходит, пока App открыт, — уже онлайн
        this.AddUser(MetricTypeEnum.OnlineUsers, userId, true);
        this.Add(MetricTypeEnum.AppExceptions, dto.Exceptions);
    }

    /// <summary>
    ///     Забрать минуты раньше текущей, сведённые в одно значение </summary>
    public List<Metric> TakeClosed() {
        var now = RoundMinute(_utcNow());
        List<KeyValuePair<DateTime, Bucket>> closed;
        lock (_lock) {
            closed = _buckets.Where(e => e.Key < now).ToList();
            foreach (var item in closed) {
                _buckets.Remove(item.Key);
            }
        }
        var result = new List<Metric>();
        foreach (var (time, bucket) in closed) {
            foreach (var (type, values) in bucket.Values) {
                var value = MetricConverter.TypeToKind(type) == MetricKindEnum.Median ? Median(values) : values.Sum();
                result.Add(new Metric { DateTime = time, MetricId = type, Value = value });
            }
            foreach (var (type, users) in bucket.Users) {
                result.Add(new Metric { DateTime = time, MetricId = type, Value = users.Count(e => e.Value) });
            }
        }
        return result;
    }

    #region [ private ]

    private Bucket Current() {
        var now = RoundMinute(_utcNow());
        if (_buckets.TryGetValue(now, out var bucket) == false) {
            _buckets[now] = bucket = new Bucket();
        }
        return bucket;
    }

    private static int Median(List<int> values) {
        var sorted = values.OrderBy(e => e).ToList();
        var middle = sorted.Count / 2;
        return sorted.Count % 2 == 0 ? (sorted[middle - 1] + sorted[middle]) / 2 : sorted[middle];
    }

    #endregion [ private ]
}
