using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.DTOs.Read;

public record KriReadingDto(
    Guid Id,
    Guid KriDefinitionId,
    double Value,
    double NormalizedScore,
    RiskLevel RiskLevel,
    DateTime Timestamp,
    bool IsSimulated
);
