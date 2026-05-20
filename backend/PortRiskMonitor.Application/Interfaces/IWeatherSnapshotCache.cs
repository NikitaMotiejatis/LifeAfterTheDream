using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IWeatherSnapshotCache
{
    void UpdateSnapshot(WeatherSnapshot snapshot);
    WeatherSnapshot GetLatest();
}
