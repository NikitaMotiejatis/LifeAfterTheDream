using System.ComponentModel.DataAnnotations;

namespace PortRiskMonitor.Data.Entities;

// NFR: Cross-cutting / Interceptors — written by BusinessLogicAuditFilter.
// Records WHO did WHAT, WHEN, duration, and outcome for every controller action.
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string ClassName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string MethodName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string UserIdentifier { get; set; } = "anonymous";

    [MaxLength(100)]
    public string Permissions { get; set; } = "operator";

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public long DurationMs { get; set; }

    public bool Success { get; set; } = true;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    [MaxLength(500)]
    public string RequestPath { get; set; } = string.Empty;

    public int ResponseStatusCode { get; set; }
}
