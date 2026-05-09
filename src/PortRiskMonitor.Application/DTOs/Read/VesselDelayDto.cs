using System;

namespace PortRiskMonitor.Application.DTOs.Read;

public record VesselDelayDto(
    string   VesselName,
    string   VesselType,
    DateTime ScheduledTime,
    DateTime? ActualTime,
    double   DelayHours,
    string   Status
);
