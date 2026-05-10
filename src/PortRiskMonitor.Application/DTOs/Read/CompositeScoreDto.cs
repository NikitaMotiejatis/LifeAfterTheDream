using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.DTOs.Read;

public record CompositeScoreDto(
    double Score,
    RiskLevel Level,
    string Description
);
