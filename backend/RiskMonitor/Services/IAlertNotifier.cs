using RiskMonitor.Entities;

namespace RiskMonitor.Services;

public interface IAlertNotifier
{
    Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default);
}
