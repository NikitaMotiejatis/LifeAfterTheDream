// ============================================================
// KriRepository.cs — EF Core implementation of IKriRepository
//
// This is the only place in the codebase that directly accesses AppDbContext.
// All queries use EF Core LINQ — no raw SQL string interpolation, ever.
// ============================================================

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.Repositories;

public class KriRepository : IKriRepository
{
    private readonly AppDbContext _db;

    public KriRepository(AppDbContext db) => _db = db;

    // ── KRI definitions ────────────────────────────────────────────────────────

    public async Task<Kri> GetKri()
        => new Kri
        {
            Name = "",
            Description = "",
        };
    public async Task<IEnumerable<KriReading>> GetAllReadings()
        => await _db.KriReadings.ToListAsync();

    public async Task<IEnumerable<Kri>> GetAllAsync()
        => await _db.Kris
            .OrderByDescending(k => k.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Kri?> GetByIdAsync(Guid id)
        => await _db.Kris.FirstOrDefaultAsync(k => k.Id == id);

    public async Task<Kri?> GetBySlugAsync(string slug)
        => await _db.Kris
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Slug == slug);

    public async Task<Kri> CreateAsync(Kri kri)
    {
        kri.Id = Guid.NewGuid();
        kri.CreatedAt = DateTime.UtcNow;
        _db.Kris.Add(kri);
        await _db.SaveChangesAsync();
        return kri;
    }

    public async Task<Kri> UpdateAsync(Kri kri)
    {
        _db.Kris.Update(kri);
        await _db.SaveChangesAsync();
        return kri;
    }

    public async Task DeleteAsync(Guid id)
    {
        var kri = await _db.Kris.FindAsync(id);
        if (kri is null) return;
        _db.Kris.Remove(kri);
        await _db.SaveChangesAsync();
    }

    // ── Readings ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<KriReading>> GetLatestReadingsAsync()
    {
        var latestTimestamps = _db.KriReadings
            .GroupBy(r => r.KriId)
            .Select(g => new { KriId = g.Key, Timestamp = g.Max(r => r.Timestamp) });

        return await _db.KriReadings
            .Include(r => r.Kri)
            .Where(r => latestTimestamps
                .Any(l => l.KriId == r.KriId && l.Timestamp == r.Timestamp))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KriReading>> GetReadingsAsync(
        Guid kriId, DateTime from, DateTime to, int take = 1000)
        => await _db.KriReadings
            .Where(r => r.KriId == kriId && r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();

    public async Task<KriReading> AddReadingAsync(KriReading reading)
    {
        reading.Id = Guid.NewGuid();
        _db.KriReadings.Add(reading);
        await _db.SaveChangesAsync();
        return reading;
    }

    public async Task AddReadingsBatchAsync(IEnumerable<KriReading> readings)
    {
        // Assign IDs only — do NOT override Timestamp here.
        // Callers (SeedData, background services) set their own timestamps.
        var list = readings.Select(r =>
        {
            r.Id = Guid.NewGuid();
            return r;
        }).ToList();

        await _db.KriReadings.AddRangeAsync(list);
        await _db.SaveChangesAsync();
    }
}
