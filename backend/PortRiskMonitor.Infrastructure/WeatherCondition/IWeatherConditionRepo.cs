namespace PortRiskMonitor.Infrastructure.WeatherCondition;

public interface IWeatherConditionRepo
{
    public WeatherSnapshot GetLatestWeatherSnapshot();
}
