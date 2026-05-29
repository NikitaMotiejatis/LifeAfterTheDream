using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.BackgroundServices;

public class BerthOccupancyFetcherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherFetcherService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public BerthOccupancyFetcherService(
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        ILogger<WeatherFetcherService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Berth Occupancy Background Worker starting.");

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
            _logger.LogInformation("Fetching fresh berth occupancy data from external API...");

            var newSnapshot = await FetchData();
            await AddKriReading(newSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch berth occupancy data from external API.");
        }
    }

    private async Task AddKriReading(BerthOccupancy berthOccupancy)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<IRiskMonitorRepository>()
                .AddReadingAsync("berth-occupancy", CalculateKriScore(berthOccupancy));

            _logger.LogInformation("Successfully saved berth occupancy data to the database at {Time}.", DateTime.UtcNow);
        }
    }

    private double CalculateKriScore(BerthOccupancy berthOccupancy)
        => 100.0 * (double)berthOccupancy.occupied / (double)berthOccupancy.total;

    private Task<BerthOccupancy> FetchData()
    {
        var total = (uint)Random.Shared.NextInt64(10, 20);
        var occupied = (uint)Random.Shared.NextInt64(0, total + 1);

        return Task.FromResult(new BerthOccupancy(total, occupied));
    }

    private record BerthOccupancy(uint total, uint occupied);
}
