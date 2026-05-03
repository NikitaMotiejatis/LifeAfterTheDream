using PortRiskMonitor.Application.DTOs.Enums;

namespace PortRiskMonitor.Application.DTOs.Read;

public record CompositeScoreDto(
    double       Score,        // 0–100
    RiskLevelDto Level,        // Green | Yellow | Red
    string       Description   // e.g. "Normal Operations" | "Monitor Closely" | "Action Required"
);