using Microsoft.Extensions.Logging;
using RiskMonitor.Entities;
using RiskMonitor.Services;

namespace PortRiskMonitor.Infrastructure.Notifications;

// NFR: Extensibility / Strategy — no-op IAlertNotifier used when SMS is disabled.
public class NullAlertNotifier : IAlertNotifier
{
    private readonly ILogger<NullAlertNotifier> _logger;

    public NullAlertNotifier(ILogger<NullAlertNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "RED alert (SMS disabled) — {KriName} @ {Value}: {Message}",
            alert.KriName, alert.TriggerValue, alert.Message);
        return Task.CompletedTask;
    }
}
