using Microsoft.Extensions.Logging;
using RiskMonitor.Entities;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Notifications.Decorators;

public class RetryNotifierDecorator : IAlertNotifier
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(500);

    private readonly IAlertNotifier _inner;
    private readonly ILogger<RetryNotifierDecorator> _logger;

    public RetryNotifierDecorator(IAlertNotifier inner, ILogger<RetryNotifierDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await _inner.NotifyRedAsync(alert, cancellationToken);
                if (attempt > 1)
                    _logger.LogInformation("Notifier succeeded on retry {Attempt} for {KriName}", attempt, alert.KriName);
                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                var delay = BaseDelay * attempt;
                _logger.LogWarning(ex,
                    "Notifier attempt {Attempt}/{Max} failed for {KriName} — retrying in {Delay}ms",
                    attempt, MaxAttempts, alert.KriName, delay.TotalMilliseconds);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
