// ============================================================
// IKriRepository.cs — Data Access contract for KRI operations
//
// This interface sits in the Infrastructure layer and is implemented
// by KriRepository (also in Infrastructure).
//
// The Application layer services depend on this INTERFACE, not on
// the concrete class. This is the Dependency Inversion Principle —
// it means we can swap the implementation (e.g., switch from SQLite
// to a REST API data source) without changing any service code.
//
// All methods are async — satisfies NFR: Async / non-blocking communication.
// No method holds a transaction open across calls — satisfies NFR: Data Access.
// ============================================================

using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Repositories;

public interface IKriRepository
{
    // ── Read operations ───────────────────────────────────────────────────────

    // Returns all KriDefinitions for the KRI Manager list view
    // Ordered by CreatedAt descending (newest first)
    Task<IEnumerable<KriDefinition>> GetAllAsync();

    // Returns a single KriDefinition by ID, or null if not found
    // Used by the detail view and edit form
    Task<KriDefinition?> GetByIdAsync(Guid id);

    // Returns the most recent KriReading for each KRI
    // Used by the dashboard to show current status of all KRIs
    // TODO: Optimise this with a single SQL query using ROW_NUMBER() or GROUP BY
    Task<IEnumerable<KriReading>> GetLatestReadingsAsync();

    // Returns historical readings for a single KRI within a date range
    // Used by the trend chart on the Detail view
    // Take parameter limits results (e.g., last 1000 readings to avoid huge payloads)
    Task<IEnumerable<KriReading>> GetReadingsAsync(
        Guid kriId,
        DateTime from,
        DateTime to,
        int take = 1000);

    // ── Write operations ──────────────────────────────────────────────────────

    // Creates a new KriDefinition. Throws if Name already exists (unique constraint).
    Task<KriDefinition> CreateAsync(KriDefinition kri);

    // Updates an existing KriDefinition.
    // NFR: Optimistic Locking — EF Core will throw DbUpdateConcurrencyException
    // if RowVersion doesn't match the current DB value (another user modified it).
    // The calling service should NOT catch this — let it bubble up to the controller.
    Task<KriDefinition> UpdateAsync(KriDefinition kri);

    // Soft-deletes or hard-deletes a KriDefinition.
    // TODO: Decide whether to soft-delete (set IsDeleted=true) or hard-delete.
    //       Hard delete cascades to Readings and Alerts (configured in AppDbContext).
    //       Soft delete would require filtering in all read queries.
    //       For PoC: hard delete is fine.
    Task DeleteAsync(Guid id);

    // ── Reading write operations ──────────────────────────────────────────────

    // Saves a new KriReading (called by MockDataBackgroundService and manual override endpoint)
    Task<KriReading> AddReadingAsync(KriReading reading);

    // Saves a batch of readings in a single transaction
    // More efficient than calling AddReadingAsync in a loop
    // Used by MockDataBackgroundService when updating all KRIs at once
    Task AddReadingsBatchAsync(IEnumerable<KriReading> readings);
}
