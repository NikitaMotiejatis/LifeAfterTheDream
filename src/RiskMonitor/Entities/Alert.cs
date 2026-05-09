using System.ComponentModel.DataAnnotations;

using RiskMonitor.DTOs;

namespace RiskMonitor.Entities;

public class Alert
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public RiskLevel RiskLevel { get; set; }

    public KriReading Reading { get; set; } = null!;
}
