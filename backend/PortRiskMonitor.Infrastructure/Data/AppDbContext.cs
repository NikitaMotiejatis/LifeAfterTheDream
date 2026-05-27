using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Entities;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kri> Kris => Set<Kri>();
    public DbSet<KriReading> KriReadings => Set<KriReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Kri>(entity =>
        {
            entity.ToTable("Kris");
            entity.HasKey(e => e.Id);

            entity.Property<uint>("xmin")
                .HasColumnName("xmin")
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Kris_Name");

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Kris_Slug");
        });

        modelBuilder.Entity<KriReading>(entity =>
        {
            entity.ToTable("KriReadings");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Kri)
                .WithMany(k => k.Readings)
                .HasForeignKey(e => e.KriId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.KriId, e.Timestamp })
                .HasDatabaseName("IX_KriReadings_KriId_Timestamp");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alerts");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Kri)
                .WithMany()
                .HasForeignKey(e => e.KriId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ResolvedAt)
                .HasDatabaseName("IX_Alerts_ResolvedAt");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ExecutedAt)
                .HasDatabaseName("IX_AuditLogs_ExecutedAt");

            entity.HasIndex(e => e.UserIdentifier)
                .HasDatabaseName("IX_AuditLogs_UserIdentifier");
        });
    }
}
