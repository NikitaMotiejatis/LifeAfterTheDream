using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.Application.Services;

public class AisSnapshotCache : IAisSnapshotCache
{
    private volatile AisSnapshot? _latestSnapshot;

    public void UpdateSnapshot(AisSnapshot snapshot)
        => _latestSnapshot = snapshot;

    public AisSnapshot GetLatest()
        => _latestSnapshot
            ?? throw new NotFoundException("Latest AIS snapshot does not exist");
}
