using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     Цикл с интервалом; на каждую итерацию свой scope и ServicesHost; ошибка логируется, цикл продолжается </summary>
public abstract class BaseBackgroundService : BackgroundService {

    protected readonly IServiceProvider ServiceProvider;

    protected abstract TimeSpan Interval { get; }

    /// <summary>
    ///     Имя для _l.Scope и сообщений </summary>
    protected abstract string Name { get; }

    protected BaseBackgroundService(IServiceProvider serviceProvider) {
        this.ServiceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (stoppingToken.IsCancellationRequested == false) {
            try {
                await Task.Delay(Interval, stoppingToken);
            } catch (TaskCanceledException) {
                return;
            }
            using var scope = ServiceProvider.CreateScope();
            using var logScope = _l.Scope(Name);
            var host = scope.ServiceProvider.GetRequiredService<ServicesHost>();
            try {
                await ExecuteIterationAsync(host, stoppingToken);
            } catch (TaskCanceledException) {
                return;
            } catch (Exception e) {
                _l.e($"{Name} iteration failed", e);
            }
        }
    }

    protected abstract Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken);
}
