using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherConditionService : IKriService
{
    double GetWindSpeedKnt();
    double GetWaterLevelCm();
    double GetTemperatureC();
    double GetHumidityPercent();
    string GetConditionCode();
}
