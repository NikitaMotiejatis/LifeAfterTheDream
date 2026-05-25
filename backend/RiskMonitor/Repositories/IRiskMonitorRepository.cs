using Microsoft.EntityFrameworkCore;
using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IRiskMonitorRepository
{
    IQueryable<Kri> GetAllIndicators();
    IQueryable<KriReading> GetAllReadings();
    IQueryable<Alert> GetAllAlerts();

    IQueryable<KriReading> GetKriReadings(string slug)
        => GetAllReadings()
            .Where(r => r.Kri.Slug == slug);

    Task<Kri?> GetBySlugAsync(string slug)
        => GetAllIndicators()
            .FirstOrDefaultAsync(k => k.Slug == slug);

    Task<Kri> UpdateAsync(Kri kri);

    IQueryable<KriReading> GetLatestReadings()
        => GetAllIndicators()
            .Select(k => k.Readings
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault())
            .Where(r => r != null)!;
}
