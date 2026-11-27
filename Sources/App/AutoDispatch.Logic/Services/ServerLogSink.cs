#nullable enable
using AutoCall.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoDispatch.Data.Api;
using AutoDispatch.Data.Logs;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;

namespace AutoDispatch.Logic.Services;

/// <summary>
///     Serilog-sink: записи Information+ пачками на сервер (Api/Logs). До Attach копит в очереди (MaxQueue, старое вытесняется),
///     после — шлёт раз в Interval по MaxBatch; неотправленная пачка ждёт следующей попытки (сеть, 5xx, 401 до повторного входа), отказ 4xx выбрасывает её.
///     Защита от спама: одно и то же сообщение (уровень + шаблон + тип исключения) не больше MaxPerKey в минуту, остальное
///     считается и уходит одной строкой «repeated N more times»; общий потолок MaxPerMinute, сверх — «dropped N».
///     Сам sink в лог не пишет: иначе зациклится </summary>
public sealed class ServerLogSink : ILogEventSink, IDisposable {

    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan DisposeTimeout = TimeSpan.FromSeconds(3);
    public const int MaxBatch = 100;
    public const int MaxQueue = 1000;
    public const int MaxPerKey = 5;
    public const int MaxPerMinute = 200;

    private sealed class KeyState {
        public DateTime Started;
        public int Count;
        public int Suppressed;
        public ClientLogEntry First = null!;
    }

    private readonly Func<DateTime> _now;
    private readonly Lock _lock = new();
    private readonly Queue<ClientLogEntry> _queue = new();
    private readonly Dictionary<string, KeyState> _keys = new();
    private DateTime _minuteStarted;
    private int _minuteCount;
    private int _dropped;

    private IClientLogSender? _sender;
    private List<ClientLogEntry>? _pending;
    private CancellationTokenSource? _cancellation;
    private bool _disposed;

    public ServerLogSink(Func<DateTime>? now = null) {
        _now = now ?? (() => DateTime.UtcNow);
        _minuteStarted = _now();
    }

    public int QueueLength {
        get {
            lock (_lock) {
                return _queue.Count;
            }
        }
    }

    #region [ Emit ]

    public void Emit(LogEvent logEvent) {
        if (logEvent.Level < LogEventLevel.Information) {
            return;
        }
        try {
            var now = _now();
            var entry = ToEntry(logEvent);
            lock (_lock) {
                FlushWindows(now);
                if (_minuteCount >= MaxPerMinute) {
                    _dropped++;
                    return;
                }
                var key = $"{logEvent.Level}|{logEvent.MessageTemplate.Text}|{logEvent.Exception?.GetType().Name}";
                if (_keys.TryGetValue(key, out var state) && now - state.Started < Window) {
                    state.Count++;
                    if (state.Count > MaxPerKey) {
                        state.Suppressed++;
                        return;
                    }
                } else {
                    _keys[key] = new KeyState { Started = now, Count = 1, First = entry };
                }
                _minuteCount++;
                Enqueue(entry);
            }
        } catch {
            // sink не должен ронять логгер
        }
    }

    // как в локальном файле: строки без кавычек (l), структуры как JSON (j)
    private static readonly MessageTemplateTextFormatter MessageFormatter = new("{Message:lj}");

    private static ClientLogEntry ToEntry(LogEvent logEvent) {
        using var message = new StringWriter();
        MessageFormatter.Format(logEvent, message);
        return new ClientLogEntry {
            Time = logEvent.Timestamp.UtcDateTime,
            Level = logEvent.Level.ToString(),
            Scope = Scalar(logEvent, LogProperties.Scope),
            Message = message.ToString(),
            Details = logEvent.Exception != null ? ExceptionFormatter.Format(logEvent.Exception) : Scalar(logEvent, LogProperties.Details)
        };
    }

    private static string? Scalar(LogEvent logEvent, string name) {
        return logEvent.Properties.TryGetValue(name, out var value) && value is ScalarValue scalar ? scalar.Value?.ToString() : null;
    }

    /// <summary>
    ///     Закрывает истёкшие окна: по ключам с подавленными повторами и по минутному потолку — по одной строке-итогу </summary>
    private void FlushWindows(DateTime now) {
        foreach (var (key, state) in _keys.Where(e => now - e.Value.Started >= Window).ToList()) {
            if (state.Suppressed > 0) {
                Enqueue(new ClientLogEntry {
                    Time = now, Level = state.First.Level, Scope = state.First.Scope,
                    Message = $"{state.First.Message} — repeated {state.Suppressed} more times in the last minute"
                });
            }
            _keys.Remove(key);
        }
        if (now - _minuteStarted >= Window) {
            if (_dropped > 0) {
                Enqueue(new ClientLogEntry {
                    Time = now, Level = nameof(LogEventLevel.Warning),
                    Message = $"{_dropped} log entries dropped: more than {MaxPerMinute} per minute"
                });
            }
            _minuteStarted = now;
            _minuteCount = 0;
            _dropped = 0;
        }
    }

    private void Enqueue(ClientLogEntry entry) {
        if (_queue.Count >= MaxQueue) {
            _queue.Dequeue();
        }
        _queue.Enqueue(entry);
    }

    #endregion [ Emit ]

    #region [ Send ]

    /// <summary>
    ///     Отправка начинается после входа: раньше нет токена </summary>
    public void Attach(IClientLogSender sender) {
        if (_cancellation != null) {
            return;
        }
        _sender = sender;
        _cancellation = new CancellationTokenSource();
        _ = Task.Run(() => RunAsync(_cancellation.Token));
    }

    private async Task RunAsync(CancellationToken cancellation) {
        while (!cancellation.IsCancellationRequested) {
            try {
                await Task.Delay(Interval, cancellation);
                await SendAsync(cancellation);
            } catch (OperationCanceledException) {
                return;
            } catch {
                // следующая попытка через Interval
            }
        }
    }

    /// <summary>
    ///     Одна попытка: неотправленная пачка либо очередная из очереди. false — пачка осталась ждать </summary>
    public async Task<bool> SendAsync(CancellationToken cancellation = default) {
        if (_sender == null) {
            return false;
        }
        List<ClientLogEntry> batch;
        lock (_lock) {
            FlushWindows(_now());
            if (_pending == null) {
                _pending = [];
                while (_pending.Count < MaxBatch && _queue.Count > 0) {
                    _pending.Add(_queue.Dequeue());
                }
            }
            batch = _pending;
        }
        if (batch.Count == 0) {
            _pending = null;
            return true;
        }
        var result = await _sender.SendAsync(new ClientLogBatch { Session = AppSession.Id, Entries = batch }, cancellation);
        if (result == ClientLogSendResult.Unavailable) {
            return false;
        }
        _pending = null;
        return true;
    }

    /// <summary>
    ///     Вызывается Serilog.Log.CloseAndFlush при выходе и падении: остаток уходит синхронно, не дольше DisposeTimeout </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _disposed = true;
        _cancellation?.Cancel();
        if (_sender == null) {
            return;
        }
        try {
            using var timeout = new CancellationTokenSource(DisposeTimeout);
            SendAsync(timeout.Token).Wait(timeout.Token);
        } catch {
            // остаток пропадает — лучше, чем висеть при выходе
        }
    }

    #endregion [ Send ]
}
