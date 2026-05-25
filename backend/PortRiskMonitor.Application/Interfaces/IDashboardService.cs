using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IDasboardService
{
    Task<PortStatusDto> GetPortStatus(string preset, string? from, string? to);
    Task<WeatherSnapshot> GetWeather();
    Task<IEnumerable<KriCardDto>> GetKriCards(string preset, string? from, string? to);
    Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame);
    Task<AisSnapshot> GetAis();
}
