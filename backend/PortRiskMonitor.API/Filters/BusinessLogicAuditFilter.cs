// NFR: Cross-cutting / Interceptors
// Global action filter that audits every controller action without modifying business logic code.
// Registered conditionally in Program.cs via appsettings.json "Auditing:Enabled".

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using PortRiskMonitor.Infrastructure.Entities;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.API.Filters;

public class BusinessLogicAuditFilter : IAsyncActionFilter
{
    private readonly IAuditLogRepository _auditRepo;
    private readonly ILogger<BusinessLogicAuditFilter> _logger;

    public BusinessLogicAuditFilter(
        IAuditLogRepository auditRepo,
        ILogger<BusinessLogicAuditFilter> logger)
    {
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        var className = context.Controller.GetType().Name;
        var methodName = context.ActionDescriptor.DisplayName ?? "Unknown";
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.Value ?? "";

        string userIdentifier = "anonymous";
        var httpContext = context.HttpContext;

        if (httpContext.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
        {
            userIdentifier = httpContext.User.Identity.Name ?? "authenticated_user";
        }
        else if (httpContext.Session != null && !string.IsNullOrEmpty(httpContext.Session.Id))
        {
            userIdentifier = $"Session: {httpContext.Session.Id}";
        }

        var executedContext = await next();

        stopwatch.Stop();

        var success = executedContext.Exception == null;
        var errorMessage = executedContext.Exception?.Message;
        var statusCode = success
            ? (executedContext.Result as Microsoft.AspNetCore.Mvc.ObjectResult)?.StatusCode ?? 200
            : 500;

        var auditLog = new AuditLog
        {
            ClassName = className,
            MethodName = methodName,
            UserIdentifier = userIdentifier,
            ExecutedAt = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Success = success,
            ErrorMessage = errorMessage,
            HttpMethod = httpMethod,
            RequestPath = path,
            ResponseStatusCode = statusCode
        };

        try
        {
            await _auditRepo.WriteAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Audit failure must not break the actual request
            _logger.LogError(ex,
                "Failed to write audit log for {method} {path}",
                httpMethod, path);
        }

        _logger.LogInformation(
            "AUDIT | {user} | {class}.{method} | {httpMethod} {path} | {status} | {duration}ms",
            userIdentifier, className, methodName, httpMethod, path, statusCode, stopwatch.ElapsedMilliseconds);
    }
}
