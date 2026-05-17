using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherFetcherService
{
    Task<WeatherSnapshot> GetLatestWeatherSnapshot();

    async Task<double> GetWindSpeedKnt() => (await GetLatestWeatherSnapshot()).WindSpeedKnt;
    async Task<double> GetWaterLevelCm() => (await GetLatestWeatherSnapshot()).WaterLevelCm;
    async Task<double> GetTemperatureC() => (await GetLatestWeatherSnapshot()).TemperatureC;
    async Task<double> GetHumidityPercent() => (await GetLatestWeatherSnapshot()).HumidityPercent;
    async Task<string> GetConditionCode() => (await GetLatestWeatherSnapshot()).ConditionCode;
}
