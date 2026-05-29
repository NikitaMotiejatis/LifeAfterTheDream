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

    Task<Kri> UpdateAsync(Kri kri, uint? originalXmin = null);

    Task<KriReading> AddReadingAsync(KriReading newReading);
    async Task<KriReading> AddReadingAsync(string slug, double value)
    {
        var kri = await GetBySlugAsync(slug)
            ?? throw new KeyNotFoundException("Could not find weather kri.");

        var newReading = new KriReading
        {
            Value = value,
            KriId = kri.Id,
            Kri = kri,
        };
        return await AddReadingAsync(newReading);
    }

    IQueryable<KriReading> GetLatestReadings()
    {
        var now = DateTime.UtcNow;
        return GetAllIndicators()
            .Select(k => k.Readings
                .Where(r => r.Timestamp <= now)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault())
            .Where(r => r != null)!;
    }
}
