using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.BackgroundServices;

public class AlertEvaluationBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    private readonly IServiceProvider _services;
    private readonly ILogger<AlertEvaluationBackgroundService> _logger;

    public AlertEvaluationBackgroundService(
        IServiceProvider services,
        ILogger<AlertEvaluationBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertEvaluationBackgroundService started (interval: {Interval})", Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var alerting = scope.ServiceProvider.GetRequiredService<IAlertingService>();
                await alerting.EvaluateAllLatestAsync(stoppingToken);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert evaluation cycle failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) { }
        }
    }
}
