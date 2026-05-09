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

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Data;

namespace PortRiskMonitor.Application.Services;

internal record WeatherSnapshot
{
    public ushort WindSpeedKnt { get; init; }
    public float WaterLevelCm { get; init; }
    public sbyte TemperatureC { get; init; }
    public byte HumidityPercent { get; init; }
    public string ConditionCode { get; init; } = "clear";
    public DateTime FetchedAt { get; init; }
}

public class WeatherConditionService : IWeatherConditionService
{
    // ── Operational thresholds ────────────────────────────────────────────────
    private const float MaxWindKnt = 25f;   // crane halt
    private const float WaterLevelBaseline = 490f;  // cm, Baltic datum at Klaipėda
    private const float MaxWaterDeviation = 60f;   // cm deviation → score 100

    // ── Score band thresholds ─────────────────────────────────────────────────
    public const float GreenMax = 33f;
    public const float YellowMax = 66f;

    // ── API endpoints ─────────────────────────────────────────────────────────
    private const string KlaipedaPortUrl = "https://portofklaipeda.lt/wp-json/api/meteo_data?method=";
    private const string PortWindUrl = KlaipedaPortUrl + "wind_speed";
    private const string PortTempUrl = KlaipedaPortUrl + "air_temparature";
    private const string PortPressureUrl = KlaipedaPortUrl + "air_presure";
    private const string HydroUrl = "https://api.meteo.lt/v1/hydro-stations/klaipedos-juru-uosto-vms/observations/measured/latest";
    private const string ConditionUrl = "https://api.meteo.lt/v1/stations/klaipedos-ams/observations/latest";

    private readonly HttpClient _httpClient;
    private readonly IRiskScoreEngine _riskCalculator;
    private readonly AppDbContext _db;
    private readonly ILogger<WeatherConditionService> _logger;

    // 10-minute cache — weather changes slowly
    private WeatherSnapshot? _cachedSnapshot;
    private DateTime _cacheExpiry = DateTime.MinValue;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WeatherConditionService(
        HttpClient httpClient,
        IRiskScoreEngine riskCalculator,
        AppDbContext db,
        ILogger<WeatherConditionService> logger)
    {
        _httpClient = httpClient;
        _riskCalculator = riskCalculator;
        _db = db;
        _logger = logger;
    }

    // ── IIndicatorScore ────────────────────────────────────────────────
    double IIndicatorScore.GetScoreValue() => CalculateScore(GetCachedSnapshot());

    string IIndicatorScore.FormatScore(double score)
    {
        var level = _riskCalculator.EvaluateRiskLevel(score, GreenMax, YellowMax, true);
        var snapshot = GetCachedSnapshot();
        return $"Weather Risk: {score:F0}/100 ({level}) | " +
               $"Wind: {snapshot.WindSpeedKnt}m/s | " +
               $"Water: {snapshot.WaterLevelCm:F0}cm | " +
               $"{snapshot.ConditionCode}";
    }

    ICollection<(DateTime Timestamp, double Score)> IIndicatorScore.GetScores(DateTime? from = null, DateTime? to = null)
    {
        // TODO: Query KriReadings table filtered to Weather KRI definition ID
        throw new NotImplementedException("TODO: wire up KRI definition ID");
    }

    // ── IWeatherConditionService ──────────────────────────────────────────────
    ushort IWeatherConditionService.GetWindSpeedKnt() => GetCachedSnapshot().WindSpeedKnt;
    float IWeatherConditionService.GetWaterLevelCm() => GetCachedSnapshot().WaterLevelCm;
    sbyte IWeatherConditionService.GetTemperatureC() => GetCachedSnapshot().TemperatureC;
    byte IWeatherConditionService.GetHumidityPercent() => GetCachedSnapshot().HumidityPercent;
    string IWeatherConditionService.GetConditionCode() => GetCachedSnapshot().ConditionCode;

    // ── Score formula ─────────────────────────────────────────────────────────
    private static double CalculateScore(WeatherSnapshot s)
    {
        var windScore = Math.Min(s.WindSpeedKnt / MaxWindKnt * 100f, 100f);
        var waterLevelScore = Math.Min(Math.Abs(s.WaterLevelCm - WaterLevelBaseline) / MaxWaterDeviation * 100f, 100f);
        var conditionScore = MapConditionToScore(s.ConditionCode);

        return windScore * 0.50f + waterLevelScore * 0.30f + conditionScore * 0.20f;
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

    // ── Cache ─────────────────────────────────────────────────────────────────

    private WeatherSnapshot GetCachedSnapshot()
    {
        if (_cachedSnapshot != null && DateTime.UtcNow < _cacheExpiry)
            return _cachedSnapshot;

        try
        {
            _cachedSnapshot = FetchSnapshotAsync().GetAwaiter().GetResult();
            _cacheExpiry = DateTime.UtcNow.AddMinutes(10);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch weather data. Retaining last cached value.");

            if (_cachedSnapshot is null)
                throw; // no fallback available on first call
        }

        return _cachedSnapshot!;
    }

    // ── HTTP fetch ────────────────────────────────────────────────────────────

    private async Task<WeatherSnapshot> FetchSnapshotAsync()
    {
        var (windTask, tempTask, pressureTask, hydroTask, conditionTask) = (
            _httpClient.GetStringAsync(PortWindUrl),
            _httpClient.GetStringAsync(PortTempUrl),
            _httpClient.GetStringAsync(PortPressureUrl),
            _httpClient.GetStringAsync(HydroUrl),
            _httpClient.GetStringAsync(ConditionUrl)
        );

        await Task.WhenAll(windTask, tempTask, pressureTask, hydroTask, conditionTask);
        // 1. Wind — Port of Klaipėda API
        //    Response is an array of [timestamp, value] tuples (both strings).
        //    Index 0 = timestamp, index 1 = wind speed in m/s.
        var portReadings = JsonSerializer.Deserialize<string[][]>(windTask.Result, JsonOptions);
        var latestPort = portReadings?.LastOrDefault() ?? throw new InvalidOperationException("Port API returned no wind readings");
        var WindSpeedKnt = double.Parse(latestPort[1], System.Globalization.CultureInfo.InvariantCulture);

        // 2. Temperature - Port of Klaipėda API
        //    Similar format to wind speed; index 1 = air temperature in °C.
        var tempReadings = JsonSerializer.Deserialize<string[][]>(tempTask.Result, JsonOptions);
        var temperature = tempReadings?.LastOrDefault() ?? throw new InvalidOperationException("Port API returned no temperature readings");
        var temperatureC = double.Parse(temperature[1], System.Globalization.CultureInfo.InvariantCulture);

        // 3. Pressure - Port of Klaipėda API
        //    Similar format; index 1 = air pressure in hPa.
        var pressureReadings = JsonSerializer.Deserialize<string[][]>(pressureTask.Result, JsonOptions);
        var pressure = pressureReadings?.LastOrDefault() ?? throw new InvalidOperationException("Port API returned no pressure readings");
        var pressureHpa = double.Parse(pressure[1], System.Globalization.CultureInfo.InvariantCulture);

        // 4. Water level — meteo.lt hydro station
        var hydroResponse = JsonSerializer.Deserialize<MeteoLtHydroResponse>(hydroTask.Result, JsonOptions);
        var latestHydro = hydroResponse?.Observations?.LastOrDefault() ?? throw new InvalidOperationException("Hydro API returned no observations");

        // 5. Conditions — meteo.lt AMS station
        var conditionResponse = JsonSerializer.Deserialize<MeteoLtStationResponse>(conditionTask.Result, JsonOptions);
        var latestCondition = conditionResponse?.Observations?.LastOrDefault() ?? throw new InvalidOperationException("AMS API returned no observations");

        return new WeatherSnapshot
        {
            WindSpeedKnt = (ushort)Math.Round(WindSpeedKnt),
            WaterLevelCm = (float)latestHydro.WaterLevelCm,
            TemperatureC = (sbyte)Math.Round(latestCondition.AirTemperature),
            HumidityPercent = (byte)latestCondition.RelativeHumidity,
            ConditionCode = latestCondition.ConditionCode,
            FetchedAt = DateTime.UtcNow
        };
    }
}

// ── JSON response shapes ──────────────────────────────────────────────────────

// meteo.lt hydro station
internal class MeteoLtHydroResponse
{
    [JsonPropertyName("observations")]
    public List<HydroObservation>? Observations { get; set; }
}

internal class HydroObservation
{
    [JsonPropertyName("observationTimeUtc")]
    public string ObservationTimeUtc { get; set; } = "";

    [JsonPropertyName("waterLevel")]
    public double WaterLevelCm { get; set; }
}

// meteo.lt AMS station
internal class MeteoLtStationResponse
{
    [JsonPropertyName("observations")]
    public List<StationObservation>? Observations { get; set; }
}

internal class StationObservation
{
    [JsonPropertyName("observationTimeUtc")]
    public string ObservationTimeUtc { get; set; } = "";

    [JsonPropertyName("airTemperature")]
    public double AirTemperature { get; set; }

    [JsonPropertyName("relativeHumidity")]
    public int RelativeHumidity { get; set; }

    [JsonPropertyName("conditionCode")]
    public string ConditionCode { get; set; } = "clear";
}
