// ============================================================
// Alert.cs — A threshold breach event for a KRI
//
// An Alert is created when a KRI transitions from a lower risk band
// to a higher one (Green→Yellow, Yellow→Red, or Green→Red).
// It is "resolved" (ResolvedAt set) when the KRI returns to a lower band.
//
// Alerts are:
//   - Displayed in the Active Alerts panel on the dashboard
//   - Listed in the Reports view for all unresolved yellow/red alerts
//   - Written to the AuditLog when created/resolved (via the Action Filter)
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortRiskMonitor.Infrastructure.Entities;

public class Alert
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Which KRI triggered this alert ───────────────────────────────────────
    public Guid KriDefinitionId { get; set; }

    // Denormalized for display without a JOIN — avoids lazy-loading the KRI
    // just to show the alert name in the UI
    [Required, MaxLength(100)]
    public string KriName { get; set; } = string.Empty;

    // ── Alert severity ────────────────────────────────────────────────────────
    [MaxLength(10)]
    public string Level { get; set; } = "Yellow"; // "Yellow" | "Red"

    // ── Value that triggered the breach ───────────────────────────────────────
    [Column(TypeName = "REAL")]
    public double TriggerValue { get; set; }

    // ── Lifecycle timestamps ──────────────────────────────────────────────────
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    // Null = alert is still ACTIVE
    // Set = alert has been RESOLVED (KRI returned to lower band)
    public DateTime? ResolvedAt { get; set; }

    // ── Human-readable message for the alerts panel ──────────────────────────
    // Example: "Berth Occupancy Rate exceeded red threshold (90%): current value 94.2%"
    // TODO: Generate this in AlertService.CreateAlertAsync() using string interpolation
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    // ── Computed property — not stored in DB ─────────────────────────────────
    [NotMapped]
    public bool IsActive => ResolvedAt == null;

    // ── Navigation property ───────────────────────────────────────────────────
    public KriDefinition KriDefinition { get; set; } = null!;
}
