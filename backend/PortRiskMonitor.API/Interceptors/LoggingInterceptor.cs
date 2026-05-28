using Castle.DynamicProxy;

namespace PortRiskMonitor.API.Interceptors;

public class LoggingInterceptor : IAsyncInterceptor, IInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<LoggingInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public void Intercept(IInvocation invocation)
    {
        this.ToInterceptor().Intercept(invocation);
    }

    public void InterceptSynchronous(IInvocation invocation)
    {
        LogBefore(invocation);
        try
        {
            invocation.Proceed();
            LogAfter(invocation);
        }
        catch (Exception ex)
        {
            LogError(invocation, ex);
            throw;
        }
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        LogBefore(invocation);
        invocation.Proceed();

        invocation.ReturnValue = InternalInterceptAsync((Task)invocation.ReturnValue, invocation);
    }

    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        LogBefore(invocation);
        invocation.Proceed();

        invocation.ReturnValue = InternalInterceptAsync((Task<TResult>)invocation.ReturnValue, invocation);
    }

    private async Task InternalInterceptAsync(Task task, IInvocation invocation)
    {
        try
        {
            await task;
            LogAfter(invocation);
        }
        catch (Exception ex)
        {
            LogError(invocation, ex);
            throw;
        }
    }

    private async Task<TResult> InternalInterceptAsync<TResult>(Task<TResult> task, IInvocation invocation)
    {
        try
        {
            TResult result = await task;
            LogAfter(invocation);
            return result;
        }
        catch (Exception ex)
        {
            LogError(invocation, ex);
            throw;
        }
    }

    private void LogBefore(IInvocation invocation)
        => PrintLog("BEFORE", invocation);
    private void LogAfter(IInvocation invocation)
        => PrintLog("AFTER", invocation);
    private void LogError(IInvocation invocation, Exception ex)
        => PrintLog($"ERROR ({ex.Message})", invocation);

    private void PrintLog(string phase, IInvocation invocation)
    {
        var context = _httpContextAccessor.HttpContext;
        string sessionId = context == null ? "BACKGROUND_TASK" : (context.Session?.Id ?? "No_Session");
        string className = invocation.TargetType.Name;
        string methodName = invocation.Method.Name;
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

        _logger.LogInformation("[Session: {SessionId}] {Phase} -> {Class}.{Method}",
            sessionId, phase, className, methodName);
    }
}
