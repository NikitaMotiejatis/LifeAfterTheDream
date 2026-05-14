// ============================================================
// AppDbContext.cs — EF Core database context
//
// This is the single point of contact between the application and the database.
// It lives in the Infrastructure (Data Access) layer.
//
// Responsibilities:
//   - Declare which entities map to which tables (DbSet<T>)
//   - Configure entity relationships, constraints, and indexes
//   - Handle optimistic concurrency via RowVersion tokens
//
// NFR: Data Access
//   - DbContext is registered as Scoped (per HTTP request) in Program.cs
//   - Transactions begin and end within a single SaveChangesAsync() call
//   - No transaction is ever held open across user interactions
// ============================================================

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Tables ────────────────────────────────────────────────────────────────
    public DbSet<KriDefinition> KriDefinitions => Set<KriDefinition>();
    public DbSet<KriReading> KriReadings => Set<KriReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── KriDefinition configuration ───────────────────────────────────────
        modelBuilder.Entity<KriDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Optimistic concurrency token
            // EF Core adds "WHERE RowVersion = @p" to every UPDATE statement
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Index on Name for faster lookups in the KRI Manager list
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_KriDefinitions_Name");

            // Auto-update UpdatedAt before every save
            // TODO: Consider using an EF Core interceptor (SaveChangesInterceptor) instead
            //       to handle this automatically for ALL entities with UpdatedAt
        });

        // ── KriReading configuration ──────────────────────────────────────────
        modelBuilder.Entity<KriReading>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Foreign key relationship: Reading belongs to one KriDefinition
            // Cascade delete: if a KriDefinition is deleted, all its Readings are deleted too
            entity.HasOne(e => e.KriDefinition)
                .WithMany(k => k.Readings)
                .HasForeignKey(e => e.KriDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite index for the most common query pattern:
            // "Give me all readings for KRI X, ordered by time, last 30 days"
            entity.HasIndex(e => new { e.KriDefinitionId, e.Timestamp })
                .HasDatabaseName("IX_KriReadings_KriId_Timestamp");

            // TODO: For production with high read volumes, consider partitioning
            //       the KriReadings table by month or archiving old readings
        });

        // ── Alert configuration ───────────────────────────────────────────────
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.KriDefinition)
                .WithMany(k => k.Alerts)
                .HasForeignKey(e => e.KriDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for the Reports endpoint: "give me all unresolved alerts"
            // A filtered index (ResolvedAt IS NULL) would be more efficient in SQL Server
            // TODO: Add filtered index when migrating to SQL Server
            entity.HasIndex(e => e.ResolvedAt)
                .HasDatabaseName("IX_Alerts_ResolvedAt");
        });

        // ── AuditLog configuration ────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            // No foreign keys on AuditLog — it should persist even if the related
            // KRI or user is later deleted. It's an append-only audit trail.

            entity.HasIndex(e => e.ExecutedAt)
                .HasDatabaseName("IX_AuditLogs_ExecutedAt");

            entity.HasIndex(e => e.UserIdentifier)
                .HasDatabaseName("IX_AuditLogs_UserIdentifier");
        });

        // ── TODO: Seed data ───────────────────────────────────────────────────
        // Uncomment and implement SeedData when MockDataBackgroundService is done.
        // SeedData.Configure(modelBuilder);
    }

    // ── Auto-update UpdatedAt timestamps ─────────────────────────────────────
    // Called automatically before SaveChanges / SaveChangesAsync
    // TODO: Expand this to handle other auditable fields (CreatedBy, UpdatedBy)
    //       when user authentication is added
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<KriDefinition>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
