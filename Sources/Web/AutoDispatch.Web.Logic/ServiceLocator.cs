using Microsoft.Extensions.DependencyInjection;

namespace AutoDispatch.Web.Logic;

/// <summary>
///     Только для мест без DI: даёт новый scope, ServicesHost берётся из него; scope освобождает вызывающий (using) </summary>
public static class ServiceLocator {

    private static IServiceProvider _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public static IServiceScope CreateScope() {
        if (_serviceProvider == null) {
            throw new InvalidOperationException("ServiceProvider has not been initialized");
        }
        return _serviceProvider.CreateScope();
    }
}
