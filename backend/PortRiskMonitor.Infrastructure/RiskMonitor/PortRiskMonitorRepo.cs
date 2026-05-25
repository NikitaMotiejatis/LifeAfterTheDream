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

    public async Task<Kri> UpdateAsync(Kri kri)
    {
        _db.Kris.Update(kri);
        await _db.SaveChangesAsync();
        return kri;
    }
}
