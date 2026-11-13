using AutoDispatch.Web.Common;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Singleton: квота записей Api/Logs на пользователя — не больше AppSettings.ClientLogQuota за ClientLogQuotaWindow,
///     сверх — пачки отбрасываются целиком. Защита от App, который зациклился на ошибке </summary>
public class ClientLogQuota {

    private class Entry {
        public DateTime Started;
        public int Count;
        public int Dropped;
    }

    private readonly Func<DateTime> _now;
    private readonly Dictionary<Guid, Entry> _users = new();

    public ClientLogQuota(Func<DateTime> now = null) {
        this._now = now ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    ///     true — пачку писать; firstDrop — первый отказ в окне, чтобы предупреждение в лог ушло один раз </summary>
    public bool TryTake(Guid userId, int count, out bool firstDrop) {
        var now = _now();
        lock (_users) {
            if (_users.TryGetValue(userId, out var entry) == false || now - entry.Started >= AppSettings.ClientLogQuotaWindow) {
                _users[userId] = entry = new Entry { Started = now };
            }
            firstDrop = false;
            if (entry.Count + count > AppSettings.ClientLogQuota) {
                firstDrop = entry.Dropped == 0;
                entry.Dropped += count;
                return false;
            }
            entry.Count += count;
            return true;
        }
    }
}
