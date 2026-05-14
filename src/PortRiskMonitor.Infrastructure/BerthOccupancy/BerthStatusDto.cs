namespace PortRiskMonitor.Infrastructure.BerthOccupancy;

public record BerthStatusDto(
    string BerthId,
    bool IsOccupied,
    string VesselType,
    DateTime? OccupiedSince
);
