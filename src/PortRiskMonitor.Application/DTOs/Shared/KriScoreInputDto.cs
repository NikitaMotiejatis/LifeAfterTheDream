namespace PortRiskMonitor.Application.DTOs.Shared;

public record KriScoreInputDto(
    Guid   KriId,
    double NormalizedScore,
    double Weight
);