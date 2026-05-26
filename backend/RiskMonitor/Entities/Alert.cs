using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RiskMonitor.DTOs;
using RiskMonitor.Entities;

namespace RiskMonitor.Entities;

public class Alert
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid KriId { get; set; }

    [Required, MaxLength(128)]
    public string KriName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Level { get; set; } = "Yellow";

    [Column(TypeName = "REAL")]
    public double TriggerValue { get; set; }

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [NotMapped]
    public bool IsActive => ResolvedAt == null;

    public Kri Kri { get; set; } = null!;
}
