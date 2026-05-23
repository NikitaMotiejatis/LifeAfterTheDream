namespace PortRiskMonitor.Application.Interfaces;

public interface IAlertingService
{
    Task EvaluateAllLatestAsync(CancellationToken cancellationToken = default);
}
