using System.Diagnostics;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Цикл с интервалом; на каждую итерацию свой scope и ServicesHost; ошибка логируется, цикл продолжается.
///     В лог: запуск со временем до первой итерации, каждая итерация с длительностью (LogIterations), долгая — warning </summary>
public abstract class BaseBackgroundService : BackgroundService {

    protected readonly IServiceProvider ServiceProvider;

    protected abstract TimeSpan Interval { get; }

    /// <summary>
    ///     Имя для _l.Scope и сообщений </summary>
    protected abstract string Name { get; }

    /// <summary>
    ///     Запись о каждой выполненной итерации — по ним видно, что задача жива; false — у частых (метрики раз в 20 с) </summary>
    protected virtual bool LogIterations => true;

    protected BaseBackgroundService(IServiceProvider serviceProvider) {
        this.ServiceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using (_l.Scope(Name)) {
            _l.i($"{Name} started, next run in {StringExtension.ToString(Interval)}");
        }
        while (stoppingToken.IsCancellationRequested == false) {
            try {
                await Task.Delay(Interval, stoppingToken);
            } catch (TaskCanceledException) {
                return;
            }
            using var scope = ServiceProvider.CreateScope();
            using var logScope = _l.Scope(Name);
            var host = scope.ServiceProvider.GetRequiredService<ServicesHost>();
            var stopwatch = Stopwatch.StartNew();
            try {
                await ExecuteIterationAsync(host, stoppingToken);
            } catch (TaskCanceledException) {
                return;
            } catch (Exception e) {
                _l.e($"{Name} iteration failed after {StringExtension.ToString(stopwatch.Elapsed)}", e);
                continue;
            }
            if (stopwatch.Elapsed > AppSettings.BackgroundSlowIteration) {
                _l.w($"{Name} iteration took {StringExtension.ToString(stopwatch.Elapsed)}");
            } else if (LogIterations) {
                _l.i($"{Name} iteration done in {StringExtension.ToString(stopwatch.Elapsed)}");
            }
        }
    }

    protected abstract Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken);
}
