using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.BackgroundServices;

public class CustomsDwellTimeFetcherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomsDwellTimeFetcherService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public CustomsDwellTimeFetcherService(
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        ILogger<CustomsDwellTimeFetcherService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Customs Dwell Time Background Worker starting.");

        using PeriodicTimer timer = new PeriodicTimer(_period);

        do
        {
            await FetchAndStoreStatusAsync();
        }
        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task FetchAndStoreStatusAsync()
    {
        try
        {
            _logger.LogInformation("Fetching customs dwell time from external API...");

            var newSnapshot = await FetchData();
            await AddKriReading(newSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch customs dwell time from external API.");
        }
    }

    private async Task AddKriReading(CustomsDwellTime customsDwellTime)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<IRiskMonitorRepository>()
                .AddReadingAsync("customs-dwell-time", CalculateKriScore(customsDwellTime));

            _logger.LogInformation("Successfully saved customs dwell time to the database at {Time}.", DateTime.UtcNow);
        }
    }

    private double CalculateKriScore(CustomsDwellTime customsDwellTime)
        => customsDwellTime.hours;

    private Task<CustomsDwellTime> FetchData()
        => Task.FromResult(new CustomsDwellTime(
                    50.0 * Random.Shared.NextDouble()));

    private record CustomsDwellTime(double hours);
}
