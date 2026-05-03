// ============================================================
// BusinessLogicAuditFilter.cs — Cross-cutting audit interceptor
//
// NFR: Cross-cutting Functionality / Interceptors
//
// This ASP.NET Core Action Filter intercepts EVERY controller
// action in the API. It runs before and after each action to:
//   - Record WHO called it (UserIdentifier, Permissions)
//   - Record WHAT was called (ClassName, MethodName, HttpMethod, Path)
//   - Record WHEN (ExecutedAt timestamp)
//   - Record HOW LONG it took (DurationMs)
//   - Record WHETHER IT SUCCEEDED (Success, ErrorMessage)
//
// KEY REQUIREMENT SATISFIED:
//   "To enable/disable logging, no modification or recompilation
//    of the monitored business logic code should be required."
//
//   Solution: The filter is registered conditionally in Program.cs
//   based on appsettings.json "Auditing:Enabled".
//   Setting Auditing:Enabled=false skips the filter registration
//   entirely — zero performance overhead, zero code changes needed.
//
// The filter is INVISIBLE to the controllers it monitors.
// Controllers have no audit-related code at all.
// ============================================================

using Microsoft.AspNetCore.Mvc.Filters;
using PortRiskMonitor.Infrastructure.Entities;
using PortRiskMonitor.Infrastructure.Repositories;
using System.Diagnostics;

namespace PortRiskMonitor.API.Filters;

public class BusinessLogicAuditFilter : IAsyncActionFilter
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILogger<BusinessLogicAuditFilter> _logger;

    // Scoped — a fresh instance is created per HTTP request (same as controllers)
    public BusinessLogicAuditFilter(
        IAuditLogRepository auditRepo,
        ILogger<BusinessLogicAuditFilter> logger)
    {
        _auditRepo = auditRepo;
        _logger    = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext  context,
        ActionExecutionDelegate next)
    {
        // ── BEFORE the action executes ────────────────────────────────────────
        var stopwatch = Stopwatch.StartNew();

        // Capture request metadata
        var className  = context.Controller.GetType().Name;           // e.g. "KrisController"
        var methodName = context.ActionDescriptor.DisplayName ?? "Unknown"; // e.g. "UpdateKri"
        var httpMethod = context.HttpContext.Request.Method;           // GET | POST | PUT | DELETE
        var path       = context.HttpContext.Request.Path.Value ?? ""; // e.g. "/api/kris/abc-123"

        // TODO: Replace hardcoded values with real user identity when auth is added
        // var userIdentifier = context.HttpContext.User.Identity?.Name ?? "anonymous";
        var userIdentifier = "anonymous";
        var permissions    = "operator"; // TODO: Get from user claims

        // ── Execute the actual controller action ──────────────────────────────
        var executedContext = await next();

        // ── AFTER the action executes ─────────────────────────────────────────
        stopwatch.Stop();

        var success      = executedContext.Exception == null;
        var errorMessage = executedContext.Exception?.Message;
        var statusCode   = success
            ? (executedContext.Result as Microsoft.AspNetCore.Mvc.ObjectResult)?.StatusCode ?? 200
            : 500;

        // ── Write audit log record ────────────────────────────────────────────
        var auditLog = new AuditLog
        {
            ClassName          = className,
            MethodName         = methodName,
            UserIdentifier     = userIdentifier,
            Permissions        = permissions,
            ExecutedAt         = DateTime.UtcNow,
            DurationMs         = stopwatch.ElapsedMilliseconds,
            Success            = success,
            ErrorMessage       = errorMessage,
            HttpMethod         = httpMethod,
            RequestPath        = path,
            ResponseStatusCode = statusCode
        };

        try
        {
            await _auditRepo.WriteAsync(auditLog);
        }
        catch (Exception ex)
        {
            // IMPORTANT: Audit log failure must NEVER break the actual request.
            // Log the error and continue — don't rethrow.
            _logger.LogError(ex,
                "Failed to write audit log for {method} {path}. This is non-fatal.",
                httpMethod, path);
        }

        // Log to structured logger as well (Serilog file sink)
        _logger.LogInformation(
            "AUDIT | {user} | {class}.{method} | {httpMethod} {path} | {status} | {duration}ms",
            userIdentifier, className, methodName, httpMethod, path, statusCode, stopwatch.ElapsedMilliseconds);
    }
}
