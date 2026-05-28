using RiskMonitor.Entities;

namespace PortRiskMonitor.Data.Alerts;

public interface IAlertRepository
{
    // Returns ALL alerts (including resolved) for the Reports view and alert history
    IQueryable<Alert> GetAllAlerts(int take = 100);

    // Returns all ACTIVE (unresolved) alerts — used by the dashboard alert panel
    IQueryable<Alert> GetActiveAlerts();
    // Returns the active alert for a specific KRI and level (null if none)
    // Used by AlertService to check if an alert already exists before creating a duplicate
    Task<Alert?> GetActiveAlertAsync(Guid kriId, string level);

    // Creates a new alert record when a KRI crosses a threshold
    Task<Alert> CreateAsync(Alert alert);

    // Marks an alert as resolved (used when a KRI returns to green)
    Task ResolveAsync(Guid alertId);

    // Returns ALL active alerts for a KRI (regardless of level)
    IQueryable<Alert> GetActiveAlertsForKri(Guid kriId);

    // Returns alerts for a specific KRI, ordered by TriggeredAt descending
    IQueryable<Alert> GetAlertsForKri(Guid kriId, int take = 10);
}
