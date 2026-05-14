// ============================================================
// IAlertRepository.cs + AlertRepository.cs
//
// Handles reading and writing Alert records.
// Follows the same patterns as IKriRepository / KriRepository.
// ============================================================

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Repositories;

// ── Interface ────────────────────────────────────────────────────────────────
public interface IAlertRepository
{
    // Returns all ACTIVE (unresolved) alerts — used by the dashboard alert panel
    Task<IEnumerable<Alert>> GetActiveAlertsAsync();

    // Returns ALL alerts (including resolved) for the Reports view and alert history
    Task<IEnumerable<Alert>> GetAllAlertsAsync(int take = 100);

    // Returns the active alert for a specific KRI and level (null if none)
    // Used by AlertService to check if an alert already exists before creating a duplicate
    Task<Alert?> GetActiveAlertAsync(Guid kriId, string level);

    // Creates a new alert record when a KRI crosses a threshold
    Task<Alert> CreateAsync(Alert alert);

    // Returns alerts for a specific KRI, ordered by TriggeredAt descending
    Task<IEnumerable<Alert>> GetAlertsForKriAsync(Guid kriId, int take = 10);
}

// ── Implementation ────────────────────────────────────────────────────────────
public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
    {
        return await _context.Alerts
            .Where(a => a.ResolvedAt == null)      // NULL = still active
            .OrderByDescending(a => a.TriggeredAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetAllAlertsAsync(int take = 100)
    {
        return await _context.Alerts
            .OrderByDescending(a => a.TriggeredAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Alert?> GetActiveAlertAsync(Guid kriId, string level)
    {
        // Used for deduplication — prevents creating a second active alert for the
        // same KRI at the same level if the KRI stays in the red/yellow band
        return await _context.Alerts
            .FirstOrDefaultAsync(a =>
                a.KriDefinitionId == kriId &&
                a.Level == level &&
                a.ResolvedAt == null);
    }

    public async Task<Alert> CreateAsync(Alert alert)
    {
        alert.Id = Guid.NewGuid();
        alert.TriggeredAt = DateTime.UtcNow;

        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        return alert;
    }

    public async Task ResolveAsync(Guid alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);

        if (alert != null)
        {
            alert.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsForKriAsync(Guid kriId, int take = 10)
    {
        return await _context.Alerts
            .Where(a => a.KriDefinitionId == kriId)
            .OrderByDescending(a => a.TriggeredAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }
}
