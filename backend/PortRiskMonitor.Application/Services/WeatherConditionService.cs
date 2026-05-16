// ============================================================
// WeatherConditionService.cs
//
// Score formula (higher = more risk):
//   windScore       = min(windSpeedKts / 35 × 100, 100)          [50%]
//   waterLevelScore = min(|waterLevel - 490| / 60 × 100, 100)    [30%]
//   conditionScore  = conditionCode mapped to 0–100              [20%]
//   SCORE = windScore×0.50 + waterLevelScore×0.30 + conditionScore×0.20
//
// Operational thresholds:
//   25 kts wind  → crane operations suspended (Klaipėda port standard)
//   ±60 cm water → full operational impact (flooding / draft restriction)
//   Baseline water level: 490 cm (Baltic chart datum, Klaipėda)
// ============================================================

using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.WeatherCondition;

namespace PortRiskMonitor.Application.Services;

public class WeatherConditionService : IWeatherConditionService
{
    // ── Operational thresholds ────────────────────────────────────────────────
    private const float MaxWindKnt = 25f;   // crane halt
    private const float WaterLevelBaseline = 490f;  // cm, Baltic datum at Klaipėda
    private const float MaxWaterDeviation = 60f;   // cm deviation → score 100

    // ── Score band thresholds ─────────────────────────────────────────────────
    public const float GreenMax = 33f;
    public const float YellowMax = 66f;

    private readonly IWeatherConditionRepo _weatherConditionRepo;
    private readonly ILogger<WeatherConditionService> _logger;

    public WeatherConditionService(
        IWeatherConditionRepo weatherConditionRepo,
        ILogger<WeatherConditionService> logger)
    {
        _weatherConditionRepo = weatherConditionRepo;
        _logger = logger;
    }

    public double GetScoreValue()
        => CalculateScore(_weatherConditionRepo.GetLatestWeatherSnapshot());

    public double GetWindSpeedKnt() => _weatherConditionRepo.GetLatestWeatherSnapshot().WindSpeedKnt;
    public double GetWaterLevelCm() => _weatherConditionRepo.GetLatestWeatherSnapshot().WaterLevelCm;
    public double GetTemperatureC() => _weatherConditionRepo.GetLatestWeatherSnapshot().TemperatureC;
    public double GetHumidityPercent() => _weatherConditionRepo.GetLatestWeatherSnapshot().HumidityPercent;
    public string GetConditionCode() => _weatherConditionRepo.GetLatestWeatherSnapshot().ConditionCode;

    private static double CalculateScore(WeatherSnapshot s)
    {
        var windScore = Math.Min(s.WindSpeedKnt / MaxWindKnt * 100f, 100f);
        var waterLevelScore = Math.Min(Math.Abs(s.WaterLevelCm - WaterLevelBaseline) / MaxWaterDeviation * 100f, 100f);
        var conditionScore = MapConditionToScore(s.ConditionCode);

        return windScore * 0.5 + waterLevelScore * 0.3 + conditionScore * 0.2;
    }

    // conditionCode values from meteo.lt match these keys directly
    private static float MapConditionToScore(string code) => code switch
    {
        // 100 — All operations suspended
        "thunderstorm" or "isolated-thunderstorms" or "thunderstorms" or "heavy-snow"
            => 100f,

        // 65 — Crane operations slowed
        "heavy-rain" or "heavy-sleet"
            => 65f,

        // 55 — Visibility-restricted navigation
        "fog"
            => 55f,

        // 30 — Minor slowdowns
        "rain" or "sleet" or "snow" or "light-rain" or "light-sleet" or "light-snow"
            => 30f,

        // 5 — Minimal impact
        "cloudy" or "overcast" or "cloudy-with-sunny-intervals" or "partly-cloudy"
            or "isolated-clouds" or "variable-cloudiness"
            => 5f,

        // 0 — Negligible
        "clear" or _
            => 0f
    };

}

