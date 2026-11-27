#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoDispatch.Data.Api;
using AutoDispatch.Data.Metrics;
using Serilog;

namespace AutoDispatch.Logic.Services;

/// <summary>
///     Раз в 30 с отправляет на сервер (Api/Metrics) состояние App: сам отчёт — «App открыт», плюс число необработанных исключений.
///     Копия MetricsReporter AutoCall V3 без сорсов DAT/Truckstop (их в App нет). Не ушёл — потерян </summary>
public sealed class MetricsReporter : IDisposable {

    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly MetricsApi _api;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellation;

    public MetricsReporter(MetricsApi api, ILogger logger) {
        _api = api;
        _logger = logger;
    }

    public void Start() {
        if (_cancellation != null) {
            return;
        }
        _cancellation = new CancellationTokenSource();
        // счётчики, набежавшие до входа, в первый отчёт не идут
        ClientMetrics.TakeReport();
        _ = Task.Run(() => RunAsync(_cancellation.Token));
    }

    public void Dispose() {
        _cancellation?.Cancel();
        _cancellation = null;
    }

    /// <summary>
    ///     Память процесса выше — warning один раз, пока не опустится ниже MemoryWarningReset (V1 падал с OutOfMemory) </summary>
    public const long MemoryWarningMb = 1500;
    public const long MemoryWarningResetMb = 1200;

    private async Task RunAsync(CancellationToken cancellation) {
        bool failed = false;
        bool memoryWarned = false;
        while (!cancellation.IsCancellationRequested) {
            try {
                await Task.Delay(Interval, cancellation);
            } catch (OperationCanceledException) {
                return;
            }
            long memoryMb = Environment.WorkingSet / (1024 * 1024);
            if (!memoryWarned && memoryMb > MemoryWarningMb) {
                memoryWarned = true;
                _logger.Warning("High memory usage: {MemoryMb} MB", memoryMb);
            } else if (memoryWarned && memoryMb < MemoryWarningResetMb) {
                memoryWarned = false;
                _logger.Information("Memory usage back to {MemoryMb} MB", memoryMb);
            }
            try {
                bool sent = await _api.ReportAsync(ClientMetrics.TakeReport(), cancellation);
                // сбой отправки — в лог один раз, пока не восстановится: сервер может лежать долго
                if (!sent && !failed) {
                    _logger.Warning("Metrics report was not accepted by the server");
                }
                failed = !sent;
            } catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                return;
            } catch (Exception exception) {
                if (!failed) {
                    _logger.Warning(exception, "Metrics report failed");
                }
                failed = true;
            }
        }
    }
}
