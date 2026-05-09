using System;

namespace PortRiskMonitor.Application.DTOs.Enums;

public record CustomsDwellDto(
    string CargoRef,
    string CargoType,
    DateTime ArrivedAtCustoms,
    double DwellHours,
    string Status
);
