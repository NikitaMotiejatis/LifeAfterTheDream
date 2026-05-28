namespace PortRiskMonitor.API.Extensions;

public static class IConfigurationExntesions
{
    public static Type ReadTypeFromConfig<TInterface>(
        this IConfiguration configuration,
        string configKey)
        where TInterface : class
    {
        string? className = configuration[configKey];

        if (string.IsNullOrEmpty(className))
            throw new InvalidOperationException($"Configuration key '{configKey}' is missing or empty.");

        Type? type = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t =>
                    t.FullName == className
                    && typeof(TInterface).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract);

        if (type is null)
            throw new InvalidOperationException(
                $"Could not find a valid, concrete implementation of {typeof(TInterface).Name} named '{className}'.");

        return type!;
    }
}
