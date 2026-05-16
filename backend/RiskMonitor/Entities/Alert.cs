using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RiskMonitor.Entities;

using RiskMonitor.DTOs;

namespace RiskMonitor.Entities;

public class Alert
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Which KRI triggered this alert ────────────────────────────────────────
    public Guid KriId { get; set; }

    // Denormalized for display without a JOIN
    [Required, MaxLength(128)]
    public string KriName { get; set; } = string.Empty;

    // ── Severity ───────────────────────────────────────────────────────────────
    [MaxLength(10)]
    public string Level { get; set; } = "Yellow"; // "Yellow" | "Red"

    [Column(TypeName = "REAL")]
    public double TriggerValue { get; set; }

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [NotMapped]
    public bool IsActive => ResolvedAt == null;

    // ── Navigation ─────────────────────────────────────────────────────────────
    public Kri Kri { get; set; } = null!;
}
