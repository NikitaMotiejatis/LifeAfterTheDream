namespace PortRiskMonitor.Application.DTOs;

public record WeatherSnapshot(
    double WindSpeedKts,
    double WaveHeightM,
    double TemperatureC,
    double HumidityPercent,
    string Description,
    DateTime RecordedAt
);
