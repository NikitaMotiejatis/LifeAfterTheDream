using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.WeatherCondition;

public interface IWeatherConditionRepo : IKriRepository
{
    public WeatherSnapshot GetLatestWeatherSnapshot();
}
