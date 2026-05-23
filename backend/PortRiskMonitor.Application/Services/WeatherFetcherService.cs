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
        var weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(await _httpClient.GetStringAsync(weatherUrl), JsonOptions)
            ?? throw new InternalErrorException("Failed to fetch weather info");

        var marineResponse = JsonSerializer.Deserialize<MarineResponse>(await _httpClient.GetStringAsync(marineUrl), JsonOptions)
            ?? throw new InternalErrorException("Failed to fetch marine weather info");

        return new WeatherSnapshot
        {
            WindSpeedKts = weatherResponse.Current.WindSpeed10m,
            WaveHeightM = marineResponse.Current.WaveHeight,
            TemperatureC = weatherResponse.Current.Temperature2m,
            HumidityPercent = weatherResponse.Current.RelativeHumidity2m,
            Description = GetWeatherDiscription(weatherResponse.Current.WeatherCode),
            RecordedAt = weatherResponse.Current.Time,
        };
    }

    private static string GetWeatherDiscription(short wmoCode) => wmoCode switch
    {
        00 => "Clear sky",
        01 => "Mainly clear",
        02 => "Partly cloudy",
        03 => "Overcast",
        45 => "Fog",
        48 => "Depositing rime fog",
        51 => "Light drizzle",
        53 => "Moderate drizzle",
        55 => "Dense drizzle",
        56 => "Light freezing drizzle",
        57 => "Dense freezing drizzle",
        61 => "Slight rain",
        63 => "Moderate rain",
        65 => "Heavy rain",
        66 => "Light freezing rain",
        67 => "Heavy freezing rain",
        71 => "Slight snow fall",
        73 => "Moderate snow fall",
        75 => "Heavy snow fall",
        77 => "Snow grains",
        80 => "Slight rain showers",
        81 => "Moderate rain showers",
        82 => "Violent rain showers",
        85 => "Slight snow showers",
        86 => "Heavy snow showers",
        95 => "Thunderstorm: Slight or moderate",
        96 => "Thunderstorm with slight hail",
        99 => "Thunderstorm with heavy hail",
        _ => "",
    };

    private static readonly string portLatitude = "55.717330464";
    private static readonly string portLongitude = "21.10749957";

    private static readonly string weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={portLatitude}&longitude={portLongitude}&current=temperature_2m,weather_code,wind_speed_10m,relative_humidity_2m&wind_speed_unit=kn";
    private static readonly string marineUrl = $"https://marine-api.open-meteo.com/v1/marine?latitude={portLatitude}&longitude={portLongitude}&current=wave_height&wind_speed_unit=ms";

    private record MarineResponse
    {
        [JsonPropertyName("latitude")]
        public required double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public required double Longitude { get; init; }

        [JsonPropertyName("generationtime_ms")]
        public required double GenerationtimeMs { get; init; }

        [JsonPropertyName("utc_offset_seconds")]
        public required long UtcOffsetSeconds { get; init; }

        [JsonPropertyName("timezone")]
        public required string Timezone { get; init; }

        [JsonPropertyName("timezone_abbreviation")]
        public required string TimezoneAbbreviation { get; init; }

        [JsonPropertyName("elevation")]
        public required double Elevation { get; init; }

        [JsonPropertyName("current_units")]
        public required Units CurrentUnits { get; init; }

        [JsonPropertyName("current")]
        public required Measurements Current { get; init; }

        public record Units
        {
            [JsonPropertyName("time")]
            public required string Time { get; init; }

            [JsonPropertyName("interval")]
            public required string Interval { get; init; }

            [JsonPropertyName("wave_height")]
            public required string WaveHeight { get; init; }
        };

        public record Measurements
        {
            [JsonPropertyName("time")]
            public required DateTime Time { get; init; }

            [JsonPropertyName("interval")]
            public required long Interval { get; init; }

            [JsonPropertyName("wave_height")]
            public required double WaveHeight { get; init; }
        };
    };

    private record WeatherResponse
    {
        [JsonPropertyName("latitude")]
        public required double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public required double Longitude { get; init; }

        [JsonPropertyName("generationtime_ms")]
        public required double GenerationtimeMs { get; init; }

        [JsonPropertyName("utc_offset_seconds")]
        public required long UtcOffsetSeconds { get; init; }

        [JsonPropertyName("timezone")]
        public required string Timezone { get; init; }

        [JsonPropertyName("timezone_abbreviation")]
        public required string TimezoneAbbreviation { get; init; }

        [JsonPropertyName("elevation")]
        public required double Elevation { get; init; }

        [JsonPropertyName("current_units")]
        public required Units CurrentUnits { get; init; }

        [JsonPropertyName("current")]
        public required Measurements Current { get; init; }

        public record Units
        {
            [JsonPropertyName("time")]
            public required string Time { get; init; }

            [JsonPropertyName("interval")]
            public required string Interval { get; init; }

            [JsonPropertyName("temperature_2m")]
            public required string Temperature2m { get; init; }

            [JsonPropertyName("weather_code")]
            public required string WeatherCode { get; init; }

            [JsonPropertyName("wind_speed_10m")]
            public required string WindSpeed10m { get; init; }

            [JsonPropertyName("relative_humidity_2m")]
            public required string RelativeHumidity2m { get; init; }
        };

        public record Measurements
        {
            [JsonPropertyName("time")]
            public required DateTime Time { get; init; }

            [JsonPropertyName("interval")]
            public required long Interval { get; init; }

            [JsonPropertyName("temperature_2m")]
            public required double Temperature2m { get; init; }

            [JsonPropertyName("weather_code")]
            public required short WeatherCode { get; init; }

            [JsonPropertyName("wind_speed_10m")]
            public required double WindSpeed10m { get; init; }

            [JsonPropertyName("relative_humidity_2m")]
            public required short RelativeHumidity2m { get; init; }
        };
    };
}
