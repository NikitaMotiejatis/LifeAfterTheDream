using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Data.Data;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Data.RiskMonitor;

public class PortRiskMonitorRepo : IRiskMonitorRepository
{
    private AppDbContext _db;

    public PortRiskMonitorRepo(AppDbContext db)
    {
        _db = db;
    }

    public IQueryable<Kri> GetAllIndicators()
        => _db.Kris;

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings;

    public IQueryable<Alert> GetAllAlerts()
        => _db.Alerts;

    public async Task<Kri> UpdateAsync(Kri kri, uint? originalXmin = null)
    {
        var existing = await _db.Kris.FindAsync(kri.Id)
        ?? throw new InvalidOperationException($"Kri {kri.Id} not found.");

        if (originalXmin.HasValue)
            _db.Entry(existing).Property("xmin").OriginalValue = originalXmin.Value;

        existing.GreenMax = kri.GreenMax;
        existing.YellowMax = kri.YellowMax;

        await _db.SaveChangesAsync();
        _db.Entry(existing).State = EntityState.Detached;
        return existing;
    }

    public async Task<KriReading> AddReadingAsync(KriReading newReading)
    {
        if (newReading is null)
            throw new ArgumentNullException(nameof(newReading), "Reading cannot be null.");

        await _db.KriReadings.AddAsync(newReading);
        await _db.SaveChangesAsync();

        if (newReading.Kri.Slug != "port-status")
            await AddNewPortStatusValue();

        return newReading;
    }

    public async Task<KriReading> UpdateReadingAsync(KriReading incomingReading)
    {
        var existingReading = await _db.KriReadings
            .FirstOrDefaultAsync(r => r.Id == incomingReading.Id);

        if (existingReading is null)
            throw new KeyNotFoundException($"KriReading with ID {incomingReading.Id} was not found.");

        existingReading.Value = incomingReading.Value;
        existingReading.Timestamp = incomingReading.Timestamp;
        await _db.SaveChangesAsync();

        return existingReading;
    }

    private async Task AddNewPortStatusValue()
    {
        var portStatusKri = await _db.Kris.FirstAsync(kri => kri.Slug == "port-status")
            ?? throw new ArgumentNullException("Could not find port status kri");

        var now = DateTime.UtcNow;

        var latestScores = await GetAllReadings()
            .Where(r => r.Timestamp < now)
            .GroupBy(r => r.Kri.Slug)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault()?.Value ?? 0.0
            );

        var newPortStatusValue =
            0.3 * latestScores["berth-occupancy"]
            + 0.3 * latestScores["vessel-delay-rate"]
            + 0.2 * latestScores["customs-dwell-time"]
            + 0.2 * latestScores["weather-risk"];

        var newPortStatusReading = new KriReading
        {
            Value = newPortStatusValue,
            KriId = portStatusKri.Id,
            Kri = portStatusKri,
        };

        await _db.KriReadings.AddAsync(newPortStatusReading);
        await _db.SaveChangesAsync();
    }
}
