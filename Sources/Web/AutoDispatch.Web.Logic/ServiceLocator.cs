using Microsoft.Extensions.DependencyInjection;

namespace AutoDispatch.Web.Logic;

/// <summary>
///     Только для мест без DI: даёт ServicesHost в новом scope </summary>
public static class ServiceLocator {

    private static IServiceProvider _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public static ServicesHost GetHost() {
        if (_serviceProvider == null) {
            throw new InvalidOperationException("ServiceProvider has not been initialized");
        }
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ServicesHost>();
    }
}
