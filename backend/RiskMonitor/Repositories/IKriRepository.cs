using Microsoft.EntityFrameworkCore;

using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    Task<Kri> GetKri();
    IQueryable<KriReading> GetAllReadings();

    Task<KriReading?> GetLatestReading()
        => GetAllReadings()
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

    IQueryable<KriReading> GetReadings(DateTime? from, DateTime? to)
    {
        (from, to) = (from ?? DateTime.MinValue, to ?? DateTime.MaxValue);

        return GetAllReadings()
            .Where(r => from <= r.Timestamp && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp);
    }
}

// Cross-Kri administration: read the catalog, mutate thresholds, fetch latest
// readings across all metrics. Implemented by the central KriRepository only —
// per-indicator repos (BerthOccupancyRepo, etc.) don't need this surface.
public interface IKriAdminRepository
{
    Task<IEnumerable<Kri>> GetAllAsync();
    Task<Kri?> GetBySlugAsync(string slug);
    Task<Kri> UpdateAsync(Kri kri);
    Task<IEnumerable<KriReading>> GetLatestReadingsAsync();
}
