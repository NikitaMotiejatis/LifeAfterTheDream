using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskMonitor.Entities;

public class Kri
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [Required, MaxLength(1024)]
    public required string Description { get; set; }

    [MaxLength(16)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public uint xmin { get; set; }

    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();

    [Column(TypeName = "REAL")]
    public double GreenMax { get; set; }

    [Column(TypeName = "REAL")]
    public double YellowMax { get; set; }

    [Column(TypeName = "REAL")]
    public double MockBaseline { get; set; }

    [Column(TypeName = "REAL")]
    public double MockVariance { get; set; }

    [MaxLength(50)]
    public string MockPattern { get; set; } = "Sinusoidal";
}
