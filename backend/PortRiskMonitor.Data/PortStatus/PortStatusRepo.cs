using PortRiskMonitor.Data.Data;
using PortRiskMonitor.Data.Repositories;

namespace PortRiskMonitor.Data.PortStatus;

public class PortStatusRepo : KriRepository, IPortStatusRepo
{
    private readonly AppDbContext _db;

    public PortStatusRepo(AppDbContext db)
        : base(db, "port-status")
    {
        _db = db;
    }
}
