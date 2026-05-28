namespace PortRiskMonitor.API.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonFromConfig<TInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configKey)
        where TInterface : class
    {
        var implementationType = configuration.ReadTypeFromConfig<TInterface>(configKey);

        services.AddSingleton(typeof(TInterface), implementationType);

        return services;
    }

    public static IServiceCollection AddScopedFromConfig<TInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configKey)
        where TInterface : class
    {
        var implementationType = configuration.ReadTypeFromConfig<TInterface>(configKey);

        services.AddScoped(typeof(TInterface), implementationType);

        return services;
    }

    public static IServiceCollection AddTransientFromConfig<TInterface>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configKey)
        where TInterface : class
    {
        var implementationType = configuration.ReadTypeFromConfig<TInterface>(configKey);

        services.AddTransient(typeof(TInterface), implementationType);

        return services;
    }

    public static IServiceCollection AddBackgroundService(
        this IServiceCollection services,
        IConfiguration configuration,
        string configKey)
    {
        var serviceType = configuration.ReadTypeFromConfig(configKey);

        services.AddTransient(typeof(IHostedService), serviceType);

        return services;
    }

    public static IServiceCollection AddHttpBackgroundService(
        this IServiceCollection services,
        IConfiguration configuration,
        string configKey)
    {
        var serviceType = configuration.ReadTypeFromConfig(configKey);

        services.AddHttpClient(serviceType.FullName ?? "");
        services.AddTransient(typeof(IHostedService), serviceType);

        return services;
    }
}
