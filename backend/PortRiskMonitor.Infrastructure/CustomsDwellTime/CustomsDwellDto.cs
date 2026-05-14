namespace PortRiskMonitor.Infrastructure.CustomsDwellTime;

public record CustomsDwellDto(
    string CargoRef,
    string CargoType,
    DateTime ArrivedAtCustoms,
    double DwellHours,
    string Status
);
