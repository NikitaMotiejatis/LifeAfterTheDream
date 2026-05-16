using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    // ── KRI definitions ────────────────────────────────────────────────────────
    Task<IEnumerable<Kri>> GetAllAsync();
    Task<Kri?> GetByIdAsync(Guid id);
    Task<Kri?> GetBySlugAsync(string slug);

    Task<Kri> CreateAsync(Kri kri);
    Task<Kri> UpdateAsync(Kri kri);
    Task DeleteAsync(Guid id);

    // ── Readings ───────────────────────────────────────────────────────────────
    Task<IEnumerable<KriReading>> GetLatestReadingsAsync();
    Task<IEnumerable<KriReading>> GetReadingsAsync(Guid kriId, DateTime from, DateTime to, int take = 1000);
    Task<KriReading> AddReadingAsync(KriReading reading);
    Task AddReadingsBatchAsync(IEnumerable<KriReading> readings);
}
