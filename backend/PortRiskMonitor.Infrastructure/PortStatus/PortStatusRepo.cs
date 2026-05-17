using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.PortStatus;

public class PortStatusRepo : IPortStatusRepo
{
    private readonly AppDbContext _db;

    public PortStatusRepo(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Kri> GetKri()
        => _db.Kris
            .First(kri => kri.Slug == "port-status");

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings
            .Where(r => r.Kri.Slug == "port-status");
}
