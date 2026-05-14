// ============================================================
// KriDefinition.cs — Core entity: defines a Key Risk Indicator
//
// This is the main configurable object of the system.
// Users create, edit, and delete KriDefinitions via the KRI Manager UI.
//
// EF Core maps this class to the "KriDefinitions" table.
// The [Timestamp] attribute on RowVersion enables optimistic concurrency —
// if two users edit the same KRI simultaneously, EF Core will detect the
// conflict and throw DbUpdateConcurrencyException (NFR: Data Consistency).
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortRiskMonitor.Infrastructure.Entities;

public class KriDefinition
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Display properties (shown on KRI cards and in KRI Manager table) ──────
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;          // e.g. "Berth Occupancy Rate"

    [Required, MaxLength(20)]
    public string Unit { get; set; } = string.Empty;          // e.g. "%", "hours", "score"

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;   // shown in tooltip / detail view

    [MaxLength(200)]
    public string FormulaLabel { get; set; } = string.Empty;  // human-readable formula for UI display

    // ── Risk thresholds — define green/yellow/red bands ───────────────────────
    // Convention: GreenMax < YellowMax; anything above YellowMax is RED
    // Example for Berth Occupancy: GreenMax=70, YellowMax=90 → red starts at >90%
    //
    // TODO: Add validation in KriDefinitionValidator to enforce GreenMax < YellowMax
    [Column(TypeName = "REAL")]
    public double GreenMax { get; set; }    // upper bound of green (low risk) band

    [Column(TypeName = "REAL")]
    public double YellowMax { get; set; }   // upper bound of yellow (medium risk) band
                                            // red band = anything above YellowMax

    // ── Composite score weight ────────────────────────────────────────────────
    // Weight of this KRI in the Composite Risk Score calculation (0.0 to 1.0)
    // All active KRI weights should sum to 1.0
    // TODO: Add a DB constraint or service-layer validation to enforce sum = 1.0
    [Column(TypeName = "REAL")]
    public double Weight { get; set; } = 0.25; // default: equal weight for 4 KRIs

    // Direction of risk — true means a HIGHER value is MORE risky
    // e.g. BerthOccupancy: HigherIsWorse = true (90% occupancy is worse than 50%)
    // e.g. SignalStrength:  HigherIsWorse = false (high signal is good, low is risky)
    public bool HigherIsWorse { get; set; } = true;

    // ── Mock data configuration ───────────────────────────────────────────────
    // Used by MockDataBackgroundService to generate realistic simulated readings.
    // TODO: Remove these fields when connecting to real data sources.
    [Column(TypeName = "REAL")]
    public double MockBaseline { get; set; }    // the "normal" value to fluctuate around

    [Column(TypeName = "REAL")]
    public double MockVariance { get; set; }    // max deviation from baseline per tick

    [MaxLength(50)]
    public string MockPattern { get; set; } = "Sinusoidal";
    // Available patterns (implemented in MockDataBackgroundService):
    //   "Sinusoidal"   — daily cycle, peaks at shift-change hours
    //   "RandomWalk"   — gradual drift up or down over time
    //   "StepFunction" — holds steady then jumps to a new level
    //   "Spike"        — brief spikes above baseline, then returns

    // ── Audit timestamps ─────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Optimistic Concurrency Token ─────────────────────────────────────────
    // NFR: Data Consistency — Optimistic Locking
    // EF Core automatically:
    //   1. Adds "WHERE RowVersion = @originalRowVersion" to every UPDATE statement
    //   2. Throws DbUpdateConcurrencyException if the row was modified by someone else
    // The API layer catches this and returns HTTP 409 Conflict to the React frontend.
    // The React frontend then shows the ConcurrencyConflictModal to the user.
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // ── Navigation properties (EF Core relationships) ─────────────────────────
    // One KriDefinition → many KriReadings (historical values over time)
    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();

    // One KriDefinition → many Alerts (threshold breach events)
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
