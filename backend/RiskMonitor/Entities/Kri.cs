using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskMonitor.Entities;

public class Kri
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Display ───────────────────────────────────────────────────────────────
    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [Required, MaxLength(1024)]
    public required string Description { get; set; }

    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    // URL slug — used by GET /api/history/{slug}.
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    // ── Risk thresholds (GreenMax < YellowMax; above YellowMax = Red) ─────────
    [Column(TypeName = "REAL")]
    public double GreenMax { get; set; }

    [Column(TypeName = "REAL")]
    public double YellowMax { get; set; }

    [Column(TypeName = "REAL")]
    public double Weight { get; set; } = 0.25;

    public bool HigherIsWorse { get; set; } = true;

    // ── Seed / mock config (remove when real data sources are connected) ───────
    [Column(TypeName = "REAL")]
    public double MockBaseline { get; set; }

    [Column(TypeName = "REAL")]
    public double MockVariance { get; set; }

    [MaxLength(50)]
    public string MockPattern { get; set; } = "Sinusoidal"; // Available patterns: "Sinusoidal" | "RandomWalk" | "StepFunction"

    // ── Audit ──────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Optimistic concurrency ─────────────────────────────────────────────────
    // Default to empty bytes — AppDbContext configures HasDefaultValueSql("randomblob(8)")
    // so SQLite overwrites this on INSERT. Never leave null — causes NOT NULL violation.
    [Timestamp]
    public byte[] RowVersion { get; set; } = new byte[8];

    // ── Navigation ─────────────────────────────────────────────────────────────
    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();
}
