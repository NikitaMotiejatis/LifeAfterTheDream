using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.Application.Services;

public class WeatherFetcherService : BackgroundService
{
    private const string KlaipedaPortUrl = "https://portofklaipeda.lt/wp-json/api/meteo_data?method=";
    private const string PortWindUrl = KlaipedaPortUrl + "wind_speed";
    private const string PortTempUrl = KlaipedaPortUrl + "air_temparature";
    private const string PortPressureUrl = KlaipedaPortUrl + "air_presure";
    private const string HydroUrl = "https://api.meteo.lt/v1/hydro-stations/klaipedos-juru-uosto-vms/observations/measured/latest";
    private const string ConditionUrl = "https://api.meteo.lt/v1/stations/klaipedos-ams/observations/latest";

    private readonly HttpClient _httpClient;
    private readonly IWeatherSnapshotCache _cache;
    private readonly ILogger<WeatherFetcherService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public WeatherFetcherService(
        HttpClient httpClient,
        IWeatherSnapshotCache cache,
        ILogger<WeatherFetcherService> logger)
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

    private async Task<WeatherSnapshot> FetchData()
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
        var latestPort = portReadings?.LastOrDefault() ?? throw new InternalErrorException("Port API returned no wind readings");
        var WindSpeedKnt = double.Parse(latestPort[1], System.Globalization.CultureInfo.InvariantCulture);

        // 2. Temperature - Port of Klaipėda API
        //    Similar format to wind speed; index 1 = air temperature in °C.
        var tempReadings = JsonSerializer.Deserialize<string[][]>(tempTask.Result, JsonOptions);
        var temperature = tempReadings?.LastOrDefault() ?? throw new InternalErrorException("Port API returned no temperature readings");
        var temperatureC = double.Parse(temperature[1], System.Globalization.CultureInfo.InvariantCulture);

        // 3. Pressure - Port of Klaipėda API
        //    Similar format; index 1 = air pressure in hPa.
        var pressureReadings = JsonSerializer.Deserialize<string[][]>(pressureTask.Result, JsonOptions);
        var pressure = pressureReadings?.LastOrDefault() ?? throw new InternalErrorException("Port API returned no pressure readings");
        var pressureHpa = double.Parse(pressure[1], System.Globalization.CultureInfo.InvariantCulture);

        // 4. Water level — meteo.lt hydro station
        var hydroResponse = JsonSerializer.Deserialize<MeteoLtHydroResponse>(hydroTask.Result, JsonOptions);
        var latestHydro = hydroResponse?.Observations?.LastOrDefault() ?? throw new InternalErrorException("Hydro API returned no observations");

        // 5. Conditions — meteo.lt AMS station
        var conditionResponse = JsonSerializer.Deserialize<MeteoLtStationResponse>(conditionTask.Result, JsonOptions);
        var latestCondition = conditionResponse?.Observations?.LastOrDefault() ?? throw new InternalErrorException("AMS API returned no observations");

        return new WeatherSnapshot(
            WindSpeedKnt,
            latestHydro.WaterLevelCm,
            temperatureC,
            latestCondition.RelativeHumidity,
            latestCondition.ConditionCode,
            DateTime.UtcNow
        );
    }

    // meteo.lt hydro station
    private record MeteoLtHydroResponse(
        [property: JsonPropertyName("observations")] List<HydroObservation>? Observations
    );

    private record HydroObservation(
        [property: JsonPropertyName("observationTimeUtc")] string ObservationTimeUtc,
        [property: JsonPropertyName("waterLevel")] double WaterLevelCm
    );

    // meteo.lt AMS station
    private record MeteoLtStationResponse(
        [property: JsonPropertyName("observations")] List<StationObservation>? Observations
    );

    private record StationObservation(
        [property: JsonPropertyName("airTemperature")] double AirTemperature,
        [property: JsonPropertyName("relativeHumidity")] double RelativeHumidity,
        [property: JsonPropertyName("observationTimeUtc")] string ObservationTimeUtc,
        [property: JsonPropertyName("conditionCode")] string ConditionCode
    );
}
