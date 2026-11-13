using System.Collections.Concurrent;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Data.Enums;

namespace AutoDispatch.Web.Logic.Monitoring;

/// <summary>
///     Singleton: кто делал запросы и когда. Заполняется middleware на каждый аутентифицированный запрос,
///     читается страницей мониторинга, сбрасывается в UserActivity фоновым сервисом </summary>
public class OnlineTracker {

    public record Entry(Guid UserId, Guid CompanyId, string Email, LoginSourceEnum Source, DateTime LastSeen);

    private readonly ConcurrentDictionary<Guid, Entry> _entries = new();

    public void Touch(Guid userId, Guid companyId, string email, LoginSourceEnum source) {
        _entries[userId] = new Entry(userId, companyId, email, source, DateTime.UtcNow);
    }

    /// <summary>
    ///     Онлайн сейчас: запрос был не позже AppSettings.OnlineTimeout назад </summary>
    public List<Entry> GetOnline() {
        var since = DateTime.UtcNow - AppSettings.OnlineTimeout;
        return _entries.Values.Where(e => e.LastSeen >= since).OrderByDescending(e => e.LastSeen).ToList();
    }

    public List<Entry> GetSeenSince(DateTime since) {
        return _entries.Values.Where(e => e.LastSeen >= since).ToList();
    }

    /// <summary>
    ///     Убрать тех, кого не было сутки, чтобы словарь не рос </summary>
    public void Cleanup() {
        var limit = DateTime.UtcNow.AddDays(-1);
        foreach (var key in _entries.Where(e => e.Value.LastSeen < limit).Select(e => e.Key).ToList()) {
            _entries.TryRemove(key, out _);
        }
    }
}
