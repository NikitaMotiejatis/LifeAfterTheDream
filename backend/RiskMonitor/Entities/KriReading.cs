using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskMonitor.Entities;

public class KriReading
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Foreign key ────────────────────────────────────────────────────────────
    public Guid KriId { get; set; }

    // ── The measurement ────────────────────────────────────────────────────────
    [Column(TypeName = "REAL")]
    public double Value { get; set; }

    [MaxLength(10)]
    public string RiskLevel { get; set; } = "Green";

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public bool IsSimulated { get; set; } = true;

    // ── Navigation ─────────────────────────────────────────────────────────────
    public Kri Kri { get; set; } = null!;
}
