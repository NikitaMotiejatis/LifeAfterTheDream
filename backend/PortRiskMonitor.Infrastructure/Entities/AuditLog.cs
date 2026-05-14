// ============================================================
// AuditLog.cs — Audit trail record for business logic actions
//
// NFR: Cross-cutting / Interceptors
// These records are written by BusinessLogicAuditFilter (an ASP.NET Core
// Action Filter) before and after EVERY controller action executes.
//
// The filter is:
//   - Registered globally in Program.cs (no need to decorate each controller)
//   - Enabled/disabled via appsettings.json "Auditing:Enabled" flag
//   - Completely invisible to the business logic code it monitors
//
// Each record captures: WHO did WHAT, WHEN, HOW LONG it took, and WHETHER it succeeded.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace PortRiskMonitor.Infrastructure.Entities;

public class AuditLog
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // ── WHERE — which code was executed ──────────────────────────────────────
    [MaxLength(200)]
    public string ClassName { get; set; } = string.Empty;  // e.g. "KrisController"

    [MaxLength(200)]
    public string MethodName { get; set; } = string.Empty; // e.g. "UpdateKri"

    // ── WHO — user identity ───────────────────────────────────────────────────
    // In PoC: hardcoded to "anonymous" since there are no user accounts
    // TODO: Replace with HttpContext.User.Identity?.Name when auth is added
    [MaxLength(200)]
    public string UserIdentifier { get; set; } = "anonymous";

    // In PoC: hardcoded to "operator"
    // TODO: Replace with user's actual role claim when RBAC is implemented
    [MaxLength(100)]
    public string Permissions { get; set; } = "operator";

    // ── WHEN ─────────────────────────────────────────────────────────────────
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // ── HOW LONG ─────────────────────────────────────────────────────────────
    // Useful for identifying slow endpoints and performance regressions
    public long DurationMs { get; set; }

    // ── OUTCOME ───────────────────────────────────────────────────────────────
    public bool Success { get; set; } = true;

    // Populated if an exception was thrown — captures the exception message
    // Not the full stack trace (too verbose for a DB column)
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // ── HTTP context (helpful for debugging) ─────────────────────────────────
    [MaxLength(10)]
    public string HttpMethod { get; set; } = string.Empty;  // GET | POST | PUT | DELETE

    [MaxLength(500)]
    public string RequestPath { get; set; } = string.Empty; // e.g. "/api/kris/abc-123"

    public int ResponseStatusCode { get; set; }             // 200, 201, 400, 404, 409, etc.
}
