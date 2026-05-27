using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.RiskMonitor;

public class PortRiskMonitorRepo : IRiskMonitorRepository
{
    private AppDbContext _db;

    public PortRiskMonitorRepo(AppDbContext db)
    {
        _db = db;
    }

    public IQueryable<Kri> GetAllIndicators()
        => _db.Kris;

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings;

    public IQueryable<Alert> GetAllAlerts()
        => _db.Alerts;

    public async Task<Kri> UpdateAsync(Kri kri, uint? originalXmin = null)
    {
        var existing = await _db.Kris.FindAsync(kri.Id)
        ?? throw new InvalidOperationException($"Kri {kri.Id} not found.");

        if (originalXmin.HasValue)
            _db.Entry(existing).Property("xmin").OriginalValue = originalXmin.Value;

        existing.GreenMax = kri.GreenMax;
        existing.YellowMax = kri.YellowMax;

        await _db.SaveChangesAsync();
        _db.Entry(existing).State = EntityState.Detached;
        return existing;
    }
}
