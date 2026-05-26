// NFR: Security — all queries use EF Core LINQ (parameterized).
// NFR: Data Access — SaveChangesAsync completes within a single HTTP request.

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.Alerts;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Alert> GetAllAlerts(int take = 100)
        => _context.Alerts
            .OrderByDescending(a => a.TriggeredAt)
            .Take(take);

    public IQueryable<Alert> GetActiveAlerts()
        => _context.Alerts
            .Where(a => a.ResolvedAt == null)      // NULL = still active
            .OrderByDescending(a => a.TriggeredAt);

    public async Task<Alert?> GetActiveAlertAsync(Guid kriId, string level)
        => await _context.Alerts
            .OrderByDescending(a => a.TriggeredAt)
            .FirstOrDefaultAsync(a =>
                a.KriId == kriId &&
                a.Level == level &&
                a.ResolvedAt == null);

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

    public IQueryable<Alert> GetActiveAlertsForKri(Guid kriId)
        => _context.Alerts
            .Where(a => a.KriId == kriId && a.ResolvedAt == null);

    public IQueryable<Alert> GetAlertsForKri(Guid kriId, int take = 10)
        => _context.Alerts
            .Where(a => a.KriId == kriId)
            .OrderByDescending(a => a.TriggeredAt)
            .Take(take);
}
