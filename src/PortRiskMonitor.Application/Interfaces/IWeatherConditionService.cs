namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherConditionService : IIndicatorScore
{
    double GetWindSpeedKnt();
    double GetWaterLevelCm();
    double GetTemperatureC();
    double GetHumidityPercent();
    string GetConditionCode();
}
