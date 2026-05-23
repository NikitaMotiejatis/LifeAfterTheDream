namespace PortRiskMonitor.Application.DTOs;

public record WeatherSnapshot
{
    public required double WindSpeedKts { get; init; }
    public required double WaveHeightM { get; init; }
    public required double TemperatureC { get; init; }
    public required double HumidityPercent { get; init; }
    public required string Description { get; init; }
    public required DateTime RecordedAt { get; init; }
};
