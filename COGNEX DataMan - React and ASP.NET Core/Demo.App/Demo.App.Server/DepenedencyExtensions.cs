using Demo.App.Server.Hubs;
using Demo.App.Server.Services;

namespace Demo.App.Server;

internal static class DepenedencyExtensions
{
    internal static IServiceCollection Configure(this IServiceCollection services)
    {
        services.AddServices();
        services.AddWorkers();
        services.AddHubs();
        return services;
    }

    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ScannerLogger>();
        services.AddSingleton<Scanner>();
        return services;
    }

    internal static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddSingleton<Worker>();
        services.AddHostedService(provider => provider.GetRequiredService<Worker>());
        return services;
    }

    internal static IServiceCollection AddHubs(this IServiceCollection services)
    {
        services.AddSingleton<LoggingHub>();
        return services;
    }
}
