// ============================================================
// KriReading.cs — A single time-stamped measurement of a KRI
//
// Readings are inserted by:
//   a) MockDataBackgroundService (simulated — PoC only)
//   b) TODO: Real data ingestion service (production)
//   c) Manual override endpoint (for demo scenario control)
//
// Readings accumulate over time and are used for:
//   - Dashboard KRI card "current value" (latest reading per KRI)
//   - Trend charts (last 30 days of readings)
//   - Alert trigger evaluation (compared against thresholds)
//   - Report generation (readings in yellow/red bands)
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortRiskMonitor.Infrastructure.Entities;

public class KriReading
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── Foreign key to the KRI this reading belongs to ────────────────────────
    public Guid KriDefinitionId { get; set; }

    // ── The actual measured/simulated value ───────────────────────────────────
    [Column(TypeName = "REAL")]
    public double Value { get; set; }

    // ── Normalized score 0-100 (calculated at write time, not query time) ─────
    // Pre-calculating this avoids repeated threshold evaluation on every read.
    // Formula: position within green/yellow/red band mapped to 0-33/34-66/67-100
    // TODO: This is calculated by RiskScoreEngine.NormalizeScore() before saving
    [Column(TypeName = "REAL")]
    public double NormalizedScore { get; set; }

    // ── Risk level at the time of this reading ────────────────────────────────
    // Stored as string for SQLite compatibility; use the RiskLevel enum in code.
    // TODO: Map this properly with EF Core value conversion if switching to SQL Server
    [MaxLength(10)]
    public string RiskLevel { get; set; } = "Green"; // "Green" | "Yellow" | "Red"

    // ── Timestamp ─────────────────────────────────────────────────────────────
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // ── Source flag ──────────────────────────────────────────────────────────
    // Distinguishes simulated (PoC) readings from real data (production)
    // Useful for filtering or flagging in reports
    public bool IsSimulated { get; set; } = true;

    // ── Navigation property ───────────────────────────────────────────────────
    public KriDefinition KriDefinition { get; set; } = null!;
}

// ── Enum definition — used throughout the application layer ──────────────────
// Stored as string in DB ("Green", "Yellow", "Red") for readability
// TODO: Move this to Application layer if it causes circular dependency issues
public enum RiskLevel
{
    Green,  // Low risk — within acceptable operating range
    Yellow, // Medium risk — attention required, monitor closely
    Red     // High risk — action required, escalate to supervisor
}
