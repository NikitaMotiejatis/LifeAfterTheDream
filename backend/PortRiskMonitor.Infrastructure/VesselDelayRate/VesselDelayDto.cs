namespace PortRiskMonitor.Infrastructure.VesselDelayRate;

public record VesselDelayDto(
    string VesselName,
    string VesselType,
    DateTime ScheduledTime,
    DateTime? ActualTime,
    string Status
);
