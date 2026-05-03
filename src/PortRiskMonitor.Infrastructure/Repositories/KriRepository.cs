// ============================================================
// KriRepository.cs — EF Core implementation of IKriRepository
//
// This class is the ONLY place in the codebase that directly
// interacts with AppDbContext (the database).
//
// Key rules enforced here:
//   - NFR: Security — ALL queries use LINQ / EF Core parameters.
//     Raw SQL string interpolation is NEVER used. This makes SQL
//     injection structurally impossible.
//   - NFR: Data Access — SaveChangesAsync() is called exactly once
//     per method. Transactions never span across user interactions.
//   - NFR: Concurrency — RowVersion is automatically included in
//     UPDATE queries by EF Core. No extra code needed here.
// ============================================================

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Repositories;

public class KriRepository : IKriRepository
{
    private readonly AppDbContext _context;

    // Injected by the DI container (Scoped lifetime — same instance as DbContext per request)
    public KriRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Read operations ───────────────────────────────────────────────────────

    public async Task<IEnumerable<KriDefinition>> GetAllAsync()
    {
        // NFR: Security — EF Core LINQ is always parameterized under the hood.
        // No raw SQL = no SQL injection risk.
        return await _context.KriDefinitions
            .OrderByDescending(k => k.CreatedAt)
            .AsNoTracking() // Performance: don't track entities we won't modify
            .ToListAsync();
    }

    public async Task<KriDefinition?> GetByIdAsync(Guid id)
    {
        // NOTE: Do NOT use AsNoTracking() here — we might Update() this entity later,
        // and EF Core needs to track it to include RowVersion in the UPDATE statement.
        return await _context.KriDefinitions
            .FirstOrDefaultAsync(k => k.Id == id);

        // TODO: Consider a read-only overload GetByIdReadOnlyAsync() with AsNoTracking()
        //       for use cases where modification is not needed (e.g., dashboard display)
    }

    public async Task<IEnumerable<KriReading>> GetLatestReadingsAsync()
    {
        // TODO: Implement this efficiently.
        // Naive approach (N+1 problem — DO NOT USE IN PRODUCTION):
        //   var kris = await _context.KriDefinitions.ToListAsync();
        //   foreach (var kri in kris) { var latest = _context.KriReadings.Last()... }
        //
        // Correct approach: single query using GroupBy or ROW_NUMBER window function.
        // EF Core 8 supports this via GroupBy with Select:
        return await _context.KriReadings
            .GroupBy(r => r.KriDefinitionId)
            .Select(g => g.OrderByDescending(r => r.Timestamp).First())
            .Include(r => r.KriDefinition) // load the parent KRI data for display
            .AsNoTracking()
            .ToListAsync();

        // TODO: If the above GroupBy causes issues with SQLite translation,
        //       fall back to: SELECT * FROM KriReadings WHERE Id IN (
        //           SELECT Id FROM KriReadings GROUP BY KriDefinitionId
        //           HAVING Timestamp = MAX(Timestamp)
        //       )
        //       Use _context.KriReadings.FromSqlRaw() with a safe parameterized query.
    }

    public async Task<IEnumerable<KriReading>> GetReadingsAsync(
        Guid kriId,
        DateTime from,
        DateTime to,
        int take = 1000)
    {
        // NFR: Security — kriId, from, to are all typed parameters.
        // EF Core translates these to SQL parameters — not string concatenation.
        return await _context.KriReadings
            .Where(r => r.KriDefinitionId == kriId
                     && r.Timestamp >= from
                     && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp)
            .Take(take) // cap results to prevent huge payloads on busy ports
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Write operations ──────────────────────────────────────────────────────

    public async Task<KriDefinition> CreateAsync(KriDefinition kri)
    {
        kri.Id        = Guid.NewGuid();
        kri.CreatedAt = DateTime.UtcNow;
        kri.UpdatedAt = DateTime.UtcNow;

        _context.KriDefinitions.Add(kri);

        // NFR: Data Access — transaction begins and ends within this single method call.
        // SaveChangesAsync commits the transaction immediately.
        await _context.SaveChangesAsync();

        return kri;
    }

    public async Task<KriDefinition> UpdateAsync(KriDefinition kri)
    {
        // EF Core is already tracking this entity (loaded via GetByIdAsync without AsNoTracking)
        // It will automatically include the RowVersion in the UPDATE WHERE clause.
        //
        // NFR: Optimistic Locking — if another user modified this row since it was loaded,
        // EF Core will throw DbUpdateConcurrencyException here.
        // DO NOT catch it here — let it bubble up to KriService, then to KrisController,
        // which returns HTTP 409 Conflict to the React frontend.
        _context.KriDefinitions.Update(kri);
        await _context.SaveChangesAsync();

        return kri;
    }

    public async Task DeleteAsync(Guid id)
    {
        var kri = await _context.KriDefinitions.FindAsync(id);

        if (kri == null)
        {
            // Caller is responsible for handling not-found before calling Delete
            // TODO: Consider throwing a custom NotFoundException here
            return;
        }

        _context.KriDefinitions.Remove(kri);
        // Cascade delete configured in AppDbContext will also remove:
        //   - All KriReadings with KriDefinitionId = id
        //   - All Alerts with KriDefinitionId = id
        await _context.SaveChangesAsync();
    }

    public async Task<KriReading> AddReadingAsync(KriReading reading)
    {
        reading.Id        = Guid.NewGuid();
        reading.Timestamp = DateTime.UtcNow;

        _context.KriReadings.Add(reading);
        await _context.SaveChangesAsync();

        return reading;
    }

    public async Task AddReadingsBatchAsync(IEnumerable<KriReading> readings)
    {
        var readingsList = readings.ToList();

        foreach (var reading in readingsList)
        {
            reading.Id        = Guid.NewGuid();
            reading.Timestamp = DateTime.UtcNow;
        }

        // AddRange + single SaveChangesAsync = one round-trip to the DB
        // Much more efficient than N separate SaveChangesAsync calls
        await _context.KriReadings.AddRangeAsync(readingsList);
        await _context.SaveChangesAsync();
    }
}
