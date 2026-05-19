using System.Linq.Expressions;

using RiskMonitor.DTOs;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace RiskMonitor.Services;

public abstract class RiskMonitorService : IRiskMonitorService
{
    protected readonly IRiskMonitorRepository _riskMonitorRepo;

    protected RiskMonitorService(IRiskMonitorRepository riskMonitorRepo)
    {
        _riskMonitorRepo = riskMonitorRepo;
    }

    public IQueryable<KriWithReadings> GetKrisWithReadings(DateTime? from, DateTime? to, Expression<Func<Kri, bool>> includeKri)
        => _riskMonitorRepo
            .GetAllIndicators()
            .Where(includeKri)
            .Select(kri => new KriWithReadings
            {
                Kri = kri,
                Readings = kri.Readings
                    .Where(r =>
                        (from ?? DateTime.MinValue) <= r.Timestamp
                        && r.Timestamp <= (to ?? DateTime.MaxValue)
                    ),
            });
}
