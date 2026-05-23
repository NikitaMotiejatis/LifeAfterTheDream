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
        => _db.Kris
            .Include(kri => kri.Readings);

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings;

    public IQueryable<Alert> GetAllAlerts()
        => _db.Alerts;

    public IQueryable<KriReading> GetKriReadings(string slug)
        => _db.KriReadings
            .Where(r => r.Kri.Slug == slug);
}
