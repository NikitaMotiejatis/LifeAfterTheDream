using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortRiskMonitor.Application.Notifications.Decorators;
using PortRiskMonitor.Application.Notifications.Options;
using RiskMonitor.Entities;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Notifications.Dispatch;

public class ChannelDispatchingNotifier : IAlertNotifier
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<NotificationOptions> _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ChannelDispatchingNotifier> _logger;

    public ChannelDispatchingNotifier(
        IServiceProvider serviceProvider,
        IOptionsMonitor<NotificationOptions> options,
        ILoggerFactory loggerFactory,
        ILogger<ChannelDispatchingNotifier> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    // Wroks based on current config value, at each notification. On RED alerts.
    public async Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var opts = _options.CurrentValue;
        var channel = string.IsNullOrWhiteSpace(opts.Channel) ? "Off" : opts.Channel;

        IAlertNotifier chain;
        try
        {
            chain = _serviceProvider.GetRequiredKeyedService<IAlertNotifier>(channel);
        }
        catch (InvalidOperationException)
        {
            _logger.LogError("Unknown notification channel '{Channel}' — falling back to Off", channel);
            chain = _serviceProvider.GetRequiredKeyedService<IAlertNotifier>("Off");
        }

        // Apply decorators in reverse so the first name in config becomes the outermost wrapper.
        var decorators = opts.Decorators ?? new List<string>();
        for (var i = decorators.Count - 1; i >= 0; i--)
        {
            chain = decorators[i] switch
            {
                "Retry" => new RetryNotifierDecorator(chain, _loggerFactory.CreateLogger<RetryNotifierDecorator>()),
                _ => Unknown(decorators[i], chain),
            };
        }

        _logger.LogDebug(
            "Dispatching alert via channel={Channel} decorators=[{Decorators}]",
            channel, string.Join(",", decorators));

        await chain.NotifyRedAsync(alert, cancellationToken);
    }

    private IAlertNotifier Unknown(string name, IAlertNotifier passthrough)
    {
        _logger.LogWarning("Unknown decorator '{Decorator}' — ignored", name);
        return passthrough;
    }
}
