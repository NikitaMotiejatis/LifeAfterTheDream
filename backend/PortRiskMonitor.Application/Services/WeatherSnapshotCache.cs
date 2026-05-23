using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.Application.Services;

public class WeatherSnapshotCache : IWeatherSnapshotCache
{
    private volatile WeatherSnapshot? _latestSnapshot;

    public void UpdateSnapshot(WeatherSnapshot snapshot)
        => _latestSnapshot = snapshot;

    public WeatherSnapshot GetLatest()
        => _latestSnapshot
            ?? throw new NotFoundException("Latest weather snapshot does not exist");
}
