using RiskMonitor.Logic;

namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherConditionService : IKriScore<double>
{
    double GetWindSpeedKnt();
    double GetWaterLevelCm();
    double GetTemperatureC();
    double GetHumidityPercent();
    string GetConditionCode();
}
