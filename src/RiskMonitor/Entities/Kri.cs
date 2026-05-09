using System.ComponentModel.DataAnnotations;

namespace RiskMonitor.Entities;

public class Kri
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [Required, MaxLength(1024)]
    public required string Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(512)]
    public string? FormulaLabel { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<KriReading> Readings { get; set; } = new List<KriReading>();
}
