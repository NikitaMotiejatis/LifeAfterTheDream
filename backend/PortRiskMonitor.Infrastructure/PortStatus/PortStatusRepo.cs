using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.Infrastructure.PortStatus;

public class PortStatusRepo : KriRepository, IPortStatusRepo
{
    private readonly AppDbContext _db;

    public PortStatusRepo(AppDbContext db)
        : base(db, "port-status")
    {
        _db = db;
    }
}
