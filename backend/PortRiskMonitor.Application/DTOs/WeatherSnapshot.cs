namespace PortRiskMonitor.Application.DTOs;

public record WeatherSnapshot(
    double WindSpeedKnt,
    double WaterLevelCm,
    double TemperatureC,
    double HumidityPercent,
    string ConditionCode,
    DateTime RecordedAt
);
