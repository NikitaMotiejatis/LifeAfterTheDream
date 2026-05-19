using System.Linq.Expressions;

using RiskMonitor.DTOs;
using RiskMonitor.Entities;

namespace RiskMonitor.Services;

public interface IRiskMonitorService
{
    IQueryable<KriWithReadings> GetKrisWithReadings(DateTime? from, DateTime? to, Expression<Func<Kri, bool>> includeKri);
}
