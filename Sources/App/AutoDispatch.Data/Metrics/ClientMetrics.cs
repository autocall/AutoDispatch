#nullable enable
namespace AutoDispatch.Data.Metrics;

/// <summary>
///     Счётчики App за 30 с для Api/Metrics, как в AutoCall V3: исключения и телефония (звонки, ошибки, регистрация софтфона на PBX);
///     сорсов DAT в AutoDispatch нет. Имена телефонии — как CallServiceType. MetricsReporter забирает и обнуляет (TakeReport) </summary>
public static class ClientMetrics {

    private sealed class CallCounters {
        public int Calls;
        public int Errors;
    }

    private static readonly Lock _lock = new();
    private static Dictionary<string, CallCounters> _calls = [];
    private static int _exceptions;
    private static int _slowRequests;
    private static bool? _sipRegistered;

    public static void Exception() => Interlocked.Increment(ref _exceptions);

    /// <summary> Запрос к серверу дольше EngineManager.SlowRequest </summary>
    public static void SlowRequest() => Interlocked.Increment(ref _slowRequests);

    /// <summary> Звонок закончился: дозвонился или не ответили. </summary>
    public static void CallEnded(string service) {
        lock (_lock) {
            Call(service).Calls++;
        }
    }

    public static void CallFailed(string service) {
        lock (_lock) {
            Call(service).Errors++;
        }
    }

    /// <summary> Регистрация софтфона на PBX; null — не известна (MicroSIP о ней не сообщает). Состояние, не сбрасывается. </summary>
    public static void SetSipRegistered(bool? registered) {
        lock (_lock) {
            _sipRegistered = registered;
        }
    }

    /// <summary> Отчёт за прошедший интервал: счётчики обнуляются, состояние регистрации остаётся </summary>
    public static ClientMetricsReport TakeReport() {
        Dictionary<string, CallCounters> calls;
        bool? sip;
        lock (_lock) {
            calls = _calls;
            sip = _sipRegistered;
            _calls = [];
        }
        return new ClientMetricsReport {
            Calls = [.. calls.Select(e => new ClientCallMetrics { Service = e.Key, Calls = e.Value.Calls, Errors = e.Value.Errors })],
            SipRegistered = sip,
            Exceptions = Interlocked.Exchange(ref _exceptions, 0),
            SlowRequests = Interlocked.Exchange(ref _slowRequests, 0),
        };
    }

    private static CallCounters Call(string service) {
        if (!_calls.TryGetValue(service, out CallCounters? counters)) {
            _calls[service] = counters = new CallCounters();
        }
        return counters;
    }
}

/// <summary> Тело Api/Metrics (ClientMetricsDto сервера). Сам отчёт означает «App открыт». </summary>
public sealed class ClientMetricsReport {
    public List<ClientCallMetrics> Calls { get; set; } = [];
    public bool? SipRegistered { get; set; }
    public int Exceptions { get; set; }
    public int SlowRequests { get; set; }
}

public sealed class ClientCallMetrics {
    public string Service { get; set; } = "";
    public int Calls { get; set; }
    public int Errors { get; set; }
}
