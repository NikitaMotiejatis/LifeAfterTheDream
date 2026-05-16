using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RiskMonitor.Entities;

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
    [Column(TypeName = "REAL")]
    public double GreenMax { get; set; }
    [Column(TypeName = "REAL")]
    public double YellowMax { get; set; }

    [Column(TypeName = "REAL")]
    public double Weight { get; set; } = 0.25;
    public bool HigherIsWorse { get; set; } = true;

    // ── Mock data configuration ───────────────────────────────────────────────
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


    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // ── Navigation properties (EF Core relationships) ─────────────────────────
    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
