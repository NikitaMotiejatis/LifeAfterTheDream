using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RiskMonitor.Entities;

public class KriReading
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(TypeName = "REAL")]
    public double Value { get; set; }

    [Required]
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public Kri Indicator { get; set; } = null!;

    public ICollection<Alert> AlertsRaised = new List<Alert>();
}
