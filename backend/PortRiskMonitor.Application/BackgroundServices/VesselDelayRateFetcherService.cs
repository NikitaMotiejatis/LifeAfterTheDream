using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.BackgroundServices;

public class VesselDelayRateFetcherService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VesselDelayRateFetcherService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public VesselDelayRateFetcherService(
        IServiceProvider serviceProvider,
        HttpClient httpClient,
        ILogger<VesselDelayRateFetcherService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClient = httpClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vessel Delay Rate Background Worker starting.");

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
            _logger.LogInformation("Fetching fresh vessel delay rate from external API...");

            var newSnapshot = await FetchData();
            await AddKriReading(newSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch vessel delay rate from external API.");
        }
    }

    private async Task AddKriReading(VesselDelayRate vesselDelayRate)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<IRiskMonitorRepository>()
                .AddReadingAsync("vessel-delay-rate", CalculateKriScore(vesselDelayRate));

            _logger.LogInformation("Successfully saved vessel delay rate to the database at {Time}.", DateTime.UtcNow);
        }
    }

    private double CalculateKriScore(VesselDelayRate vesselDelayRate)
        => 100.0 * (double)vesselDelayRate.occupied / (double)vesselDelayRate.total;

    private Task<VesselDelayRate> FetchData()
    {
        var total = (uint)Random.Shared.NextInt64(10, 50);
        var delayed = (uint)Random.Shared.NextInt64(0, total / 4);

        return Task.FromResult(new VesselDelayRate(total, delayed));
    }

    private record VesselDelayRate(uint total, uint occupied);
}
