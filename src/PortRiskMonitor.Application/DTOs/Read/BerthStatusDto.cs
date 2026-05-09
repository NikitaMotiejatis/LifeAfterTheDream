using System;

namespace PortRiskMonitor.Application.Interfaces;

public record BerthStatusDto(
    string BerthId,
    bool IsOccupied,
    string VesselType,
    DateTime? OccupiedSince
);
