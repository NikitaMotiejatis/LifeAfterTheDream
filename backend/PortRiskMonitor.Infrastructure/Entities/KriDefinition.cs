using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.Entities;

public class KriDefinition
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FormulaLabel { get; set; } = string.Empty;

    [Column(TypeName = "REAL")]
    public double GreenMax { get; set; }
    [Column(TypeName = "REAL")]
    public double YellowMax { get; set; }

    [Column(TypeName = "REAL")]
    public double Weight { get; set; } = 0.25;
    public bool HigherIsWorse { get; set; } = true;

    [Column(TypeName = "REAL")]
    public double MockBaseline { get; set; }

    [Column(TypeName = "REAL")]
    public double MockVariance { get; set; }

    [MaxLength(50)]
    public string MockPattern { get; set; } = "Sinusoidal";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // NFR: Optimistic Locking — EF Core concurrency token for conflict detection
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
