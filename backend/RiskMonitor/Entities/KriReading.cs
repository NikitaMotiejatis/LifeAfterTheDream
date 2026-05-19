using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskMonitor.Entities;

public class KriReading
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(TypeName = "REAL")]
    public double Value { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Guid KriId { get; set; }

    public Kri Kri { get; set; } = null!;
}
