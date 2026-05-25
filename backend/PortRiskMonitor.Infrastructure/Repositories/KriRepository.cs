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

public abstract class KriRepository : IKriRepository
{
    private readonly AppDbContext _db;
    private readonly string _slug;

    protected KriRepository(
            AppDbContext db,
            string slug)
    {
        _db = db;
        _slug = slug;
    }

    public Task<Kri?> GetKri()
        => _db.Kris
            .FirstOrDefaultAsync(kri => kri.Slug == _slug);

    public Task<Kri?> GetKriWithReadings(DateTime from, DateTime to)
        => _db.Kris
            .Include(kri => kri.Readings
                .Where(r => from <= r.Timestamp && r.Timestamp <= to))
            .FirstOrDefaultAsync(kri => kri.Slug == _slug);

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings
            .Where(r => r.Kri.Slug == _slug);
}
