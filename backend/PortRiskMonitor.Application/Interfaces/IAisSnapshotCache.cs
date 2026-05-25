using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IAisSnapshotCache
{
    void UpdateSnapshot(AisSnapshot snapshot);
    AisSnapshot GetLatest();
}
