using PortRiskMonitor.Application.DTOs.Enums;

namespace PortRiskMonitor.Application.DTOs.Read;

public record KriReadingDto(
    Guid Id,
    Guid KriDefinitionId,
    double Value,
    double NormalizedScore,
    RiskLevelDto RiskLevel,
    DateTime Timestamp,
    bool IsSimulated
);
