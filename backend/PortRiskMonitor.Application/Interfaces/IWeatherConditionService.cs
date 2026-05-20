using PortRiskMonitor.Application.DTOs;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherConditionService : IKriService
{
    WeatherSnapshot GetWeather();
}
