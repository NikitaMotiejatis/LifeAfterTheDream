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
    private readonly ILogger<BerthOccupancyFetcherService> _logger;
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
        ILogger<BerthOccupancyFetcherService> logger)
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
        var now = DateTime.UtcNow;
        var baseline = 60.0;
        var variance = 30.0;

        var cycle = Math.Sin(2 * Math.PI * now.Hour / 24.0) * (variance * 0.55);
        var noise = (Random.Shared.NextDouble() - 0.5) * variance * 0.45;

        var percentage = baseline + cycle + noise;

        var total = (uint)Random.Shared.NextInt64(10, 50);
        var delayed = (uint)(0.01 * percentage * total);

        return Task.FromResult(new BerthOccupancy(total, delayed));
    }

    private record BerthOccupancy(uint total, uint occupied);
}
