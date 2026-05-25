using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.Application.BackgroundServices;

public class AisFetcherService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly IAisSnapshotCache _cache;
    private readonly ILogger<AisFetcherService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    public AisFetcherService(
        HttpClient httpClient,
        IAisSnapshotCache cache,
        ILogger<AisFetcherService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Port Status Background Worker starting.");

        using PeriodicTimer timer = new PeriodicTimer(_period);

        await FetchAndStoreStatusAsync();

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await FetchAndStoreStatusAsync();
        }
    }

    private async Task FetchAndStoreStatusAsync()
    {
        try
        {
            _logger.LogInformation("Fetching fresh port status from external API...");

            var newSnapshot = await FetchData();
            _cache.UpdateSnapshot(newSnapshot);

            _logger.LogInformation("Snapshot successfully updated in memory.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch port status from external API.");
        }
    }

    private async Task<AisSnapshot> FetchData()
    {
        var targetsResponse = await _httpClient.GetAsync("https://klaipedatraffic.lt/ais/targets.geojson");
        var targetsJson = await targetsResponse.Content.ReadAsStringAsync();
        var targets = JsonSerializer.Deserialize<object>(targetsJson)
            ?? throw new JsonException("Failed to deserialize targets");

        return new AisSnapshot
        {
            Targets = targets,
        };
    }
}
