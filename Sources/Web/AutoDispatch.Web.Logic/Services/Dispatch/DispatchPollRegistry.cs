using System.Collections.Concurrent;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Singleton: текущее ожидание Api/Dispatch/Response по пользователям (V1 статические CurrentTasks по userName).
///     Новый запрос пользователя отменяет его предыдущее ожидание — у клиента один канал событий, повисший запрос
///     после переподключения не должен держать поток. Count — пользователи в long-polling (метрики, этап 8) </summary>
public class DispatchPollRegistry {

    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _waiting = new();

    public int Count => _waiting.Count;

    /// <summary>
    ///     Отменяет предыдущее ожидание пользователя и регистрирует новое; токен связан с обрывом запроса </summary>
    public CancellationTokenSource Begin(Guid userId, CancellationToken requestAborted) {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(requestAborted);
        if (_waiting.TryRemove(userId, out var previous)) {
            try {
                previous.Cancel();
            } catch (ObjectDisposedException) {
            }
        }
        _waiting[userId] = cts;
        return cts;
    }

    /// <summary>
    ///     Снимает ожидание, если его не заместил более новый запрос </summary>
    public void End(Guid userId, CancellationTokenSource cts) {
        _waiting.TryRemove(new KeyValuePair<Guid, CancellationTokenSource>(userId, cts));
        cts.Dispose();
    }
}
