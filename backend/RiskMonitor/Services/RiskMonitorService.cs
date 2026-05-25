using System.Linq.Expressions;

using RiskMonitor.DTOs;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace RiskMonitor.Services;

public abstract class RiskMonitorService : IRiskMonitorService
{
    private readonly IRiskMonitorRepository _riskMonitorRepo;

    protected RiskMonitorService(IRiskMonitorRepository riskMonitorRepo)
    {
        _riskMonitorRepo = riskMonitorRepo;
    }

    public IQueryable<KriWithReadings> GetKrisWithFilteredReadings(DateTime? from, DateTime? to, Expression<Func<Kri, bool>> includeKri)
    {
        (from, to) = (from ?? DateTime.MinValue, to ?? DateTime.MaxValue);

        return _riskMonitorRepo
            .GetAllIndicators()
            .Where(includeKri)
            .Select(kri => new KriWithReadings
            {
                Kri = kri,
                Readings = kri.Readings.Where(r => from <= r.Timestamp && r.Timestamp <= to),
            });
    }
}
