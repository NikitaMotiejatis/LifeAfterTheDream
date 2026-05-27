using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using PortRiskMonitor.Application.Exceptions;

namespace PortRiskMonitor.API.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, title, safeMessage) = exception switch
        {
            BadInputException => (StatusCodes.Status400BadRequest, "Bad Request Input", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),

            _ => (StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred on the server. Please try again later."),
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = safeMessage,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
