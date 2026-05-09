using PortRiskMonitor.Application.DTOs.Enums;

namespace PortRiskMonitor.Application.DTOs.Read;

public record KriStatusCardDto(
    Guid KriId,
    string Name,
    string Unit,
    double CurrentValue,
    double NormalizedScore,
    RiskLevelDto RiskLevel,
    bool HasActiveAlert,
    string? TrendDirection, // "Improving" | "Worsening" | "Stable"
    DateTime LastUpdated
);
