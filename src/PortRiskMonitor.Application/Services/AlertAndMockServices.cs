// ============================================================
// AlertService.cs — Creates and resolves threshold breach alerts
// MockDataBackgroundService.cs — Simulates live KRI data
// ============================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Write;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Entities;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.Application.Services;

// ────────────────────────────────────────────────────────────
// AlertService
// ────────────────────────────────────────────────────────────
public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IRiskScoreEngine _riskEngine;

    public AlertService(IAlertRepository alertRepo, IRiskScoreEngine riskEngine)
    {
        _alertRepo = alertRepo;
        _riskEngine = riskEngine;
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var alerts = await _alertRepo.GetActiveAlertsAsync();

        return alerts.Select(MapToAlertDto);
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsForKriAsync(Guid kriId, int take = 10)
    {
        var alerts = await _alertRepo.GetAlertsForKriAsync(kriId, take);

        return alerts.Select(MapToAlertDto);
    }

    private static AlertDto MapToAlertDto(Alert entity)
    {
        return new AlertDto(
            Id: entity.Id,
            KriDefinitionId: entity.KriDefinitionId,
            KriName: entity.KriName,
            Level: entity.Level,
            TriggerValue: entity.TriggerValue,
            TriggeredAt: entity.TriggeredAt,
            ResolvedAt: entity.ResolvedAt,
            IsActive: entity.IsActive,
            Message: entity.Message
        );
    }

    public async Task EvaluateAndUpdateAlertsAsync(
        Guid kriId,
        double newValue,
        double greenMax,
        double yellowMax,
        bool higherIsWorse)
    {
        // TODO: Implement alert evaluation logic:
        //
        // STEP 1: Determine the new risk level for this value
        //   var newLevel = _riskEngine.EvaluateRiskLevel(newValue, greenMax, yellowMax, higherIsWorse);
        //
        // STEP 2: Check if there's already an active alert at the same or higher level
        //   This prevents duplicate alerts when the KRI stays in the same band.
        //
        // STEP 3: If newLevel > existing active alert level → create a new alert
        //   (e.g., KRI moved from Yellow to Red — create a Red alert)
        //
        // STEP 4: If newLevel < existing active alert level → resolve the existing alert
        //   (e.g., KRI moved from Red back to Yellow — resolve the Red alert)
        //   Optionally create a new Yellow alert.
        //
        // STEP 5: If newLevel == Green and any active alert exists → resolve all alerts
        //
        // This logic ensures:
        //   - Only one active alert per KRI per level at a time
        //   - Alerts are automatically resolved when the KRI recovers
        //   - Level escalation (Yellow → Red) is captured as a new alert

        throw new NotImplementedException("TODO: Implement EvaluateAndUpdateAlertsAsync");
    }
}

// ────────────────────────────────────────────────────────────
// MockDataBackgroundService
// NFR: Async / Reactive Programming
//   - Runs on a background thread — never blocks HTTP request handlers
//   - Uses IServiceScopeFactory to get Scoped services (repos) from a Singleton context
//   - Generates time-series KRI readings that look realistic
// ────────────────────────────────────────────────────────────
public class MockDataBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<MockDataBackgroundService> _logger;

    // In-memory state for pattern generation
    // These are fields on the Singleton BackgroundService — ok because they
    // are only accessed from the single background thread (no concurrency issue here)
    private readonly Dictionary<Guid, double> _currentValues = new();
    private int _tickCount = 0;
    private string _activeScenario = "Normal";

    public MockDataBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<MockDataBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
        _activeScenario = config["MockData:DefaultScenario"] ?? "Normal";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _config["MockData:Enabled"] != "false";
        var intervalSeconds = int.TryParse(_config["MockData:IntervalSeconds"], out var s) ? s : 30;

        if (!enabled)
        {
            _logger.LogInformation("MockDataBackgroundService is disabled");
            return;
        }

        _logger.LogInformation("MockDataBackgroundService started. Interval: {s}s", intervalSeconds);

        // Seed initial KRI definitions if the DB is empty
        if (_config["MockData:SeedOnStartup"] != "false")
        {
            await SeedDefaultKrisAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateReadingsTickAsync(stoppingToken);
                _tickCount++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log but don't crash — background service should be resilient
                _logger.LogError(ex, "Error generating mock readings on tick {tick}", _tickCount);
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task GenerateReadingsTickAsync(CancellationToken ct)
    {
        // IMPORTANT: Must create a new scope for each DB interaction.
        // BackgroundService is Singleton, but DbContext is Scoped.
        // Never inject DbContext directly into a Singleton — use IServiceScopeFactory.
        using var scope = _scopeFactory.CreateScope();
        var kriRepo = scope.ServiceProvider.GetRequiredService<IKriRepository>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        var riskEngine = scope.ServiceProvider.GetRequiredService<IRiskScoreEngine>();

        var kris = await kriRepo.GetAllAsync();

        var readings = new List<KriReading>();

        foreach (var kri in kris)
        {
            // TODO: Generate a new value based on the KRI's mock pattern
            var newValue = GenerateValue(kri);

            // TODO: Calculate risk level and normalized score using riskEngine
            // var level = riskEngine.EvaluateRiskLevel(newValue, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
            // var score = riskEngine.NormalizeScore(newValue, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);

            readings.Add(new KriReading
            {
                KriDefinitionId = kri.Id,
                Value = newValue,
                NormalizedScore = 0, // TODO: set calculated score
                RiskLevel = "Green", // TODO: set calculated level
                IsSimulated = true
            });

            // TODO: Evaluate and update alerts for the new value
            // await alertService.EvaluateAndUpdateAlertsAsync(kri.Id, newValue, ...);
        }

        await kriRepo.AddReadingsBatchAsync(readings);
        _logger.LogDebug("Generated {n} mock readings on tick {tick}", readings.Count, _tickCount);
    }

    private double GenerateValue(KriDefinition kri)
    {
        // TODO: Implement pattern-based value generation
        //
        // Apply scenario override first (for demo control panel):
        //   var scenarioOverride = GetScenarioOverride(kri.Name, _activeScenario);
        //   if (scenarioOverride.HasValue) return scenarioOverride.Value;
        //
        // Then apply pattern:
        //
        // "Sinusoidal" — daily cycle peaking at shift-change hours
        //   var hour = DateTime.UtcNow.Hour;
        //   var sineValue = kri.MockBaseline + kri.MockVariance * Math.Sin(2 * Math.PI * hour / 24.0);
        //   return Math.Max(0, sineValue + Random.Shared.NextDouble() * 2 - 1); // small noise
        //
        // "RandomWalk" — gradual drift
        //   var prev = _currentValues.GetValueOrDefault(kri.Id, kri.MockBaseline);
        //   var delta = (Random.Shared.NextDouble() * 2 - 1) * kri.MockVariance;
        //   var next = Math.Clamp(prev + delta, 0, kri.YellowMax * 1.5);
        //   _currentValues[kri.Id] = next;
        //   return next;
        //
        // "StepFunction" — hold then jump
        // "Spike" — brief spike and decay

        // Temporary: return baseline with small noise until fully implemented
        return kri.MockBaseline + (Random.Shared.NextDouble() * 2 - 1) * kri.MockVariance * 0.1;
    }

    private async Task SeedDefaultKrisAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var kriRepo = scope.ServiceProvider.GetRequiredService<IKriRepository>();

        var existing = await kriRepo.GetAllAsync();
        if (existing.Any())
        {
            _logger.LogInformation("KRI definitions already exist — skipping seed");
            return;
        }

        _logger.LogInformation("Seeding default KRI definitions...");

        // Create the 4 KRI Mock definitions
        var kris = new List<KriDefinition>
    {
        new KriDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Berth Occupancy Rate",
            Unit = "%",
            Description = "Measures how busy the berths are",
            FormulaLabel = "OccupiedTime / TotalTime * 100",
            GreenMax = 70,
            YellowMax = 90,
            Weight = 0.30,
            HigherIsWorse = true,
            MockBaseline = 65,
            MockVariance = 15,
            MockPattern = "Sinusoidal",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new KriDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Vessel Delay Rate",
            Unit = "%",
            Description = "Percentage of vessels arriving late",
            FormulaLabel = "LateVessels / TotalVessels * 100",
            GreenMax = 10,
            YellowMax = 25,
            Weight = 0.30,
            HigherIsWorse = true,
            MockBaseline = 8,
            MockVariance = 8,
            MockPattern = "RandomWalk",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new KriDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Customs Dwell Time",
            Unit = "hours",
            Description = "Average time cargo waits at customs",
            FormulaLabel = "SumWaitTime / NumberOfContainers",
            GreenMax = 24,
            YellowMax = 72,
            Weight = 0.20,
            HigherIsWorse = true,
            MockBaseline = 20,
            MockVariance = 10,
            MockPattern = "StepFunction",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        },
        new KriDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Weather Risk Score",
            Unit = "score",
            Description = "Composite score of weather risks",
            FormulaLabel = "Weighted sum of wind/wave/visibility",
            GreenMax = 20,
            YellowMax = 40,
            Weight = 0.20,
            HigherIsWorse = true,
            MockBaseline = 15,
            MockVariance = 12,
            MockPattern = "Spike",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    };

        // Save each Mock KRI to the database
        foreach (var kri in kris)
        {
            await kriRepo.CreateAsync(kri);
            _logger.LogInformation($"Seeded KRI: {kri.Name}");
        }

        _logger.LogInformation("All KRI definitions seeded successfully");
    }

    // Called by POST /api/scenarios to switch the active demo scenario
    public void SetScenario(string scenarioName)
    {
        _activeScenario = scenarioName;
        _logger.LogInformation("Demo scenario changed to: {scenario}", scenarioName);
    }

    // TODO: Implement scenario override lookup
    // Returns a fixed value for a given KRI in the current scenario, or null for normal generation
    // private double? GetScenarioOverride(string kriName, string scenario)
    // {
    //     return (kriName, scenario) switch
    //     {
    //         ("Berth Occupancy Rate",  "MildCongestion") => 75.0,
    //         ("Berth Occupancy Rate",  "FullRedAlert")   => 96.0,
    //         ("Vessel Delay Rate",     "StormEvent")     => 32.0,
    //         ("Weather Risk Score",    "StormEvent")     => 67.0,
    //         ("Customs Dwell Time",    "CustomsCrisis")  => 82.0,
    //         _ => null
    //     };
    // }
}
