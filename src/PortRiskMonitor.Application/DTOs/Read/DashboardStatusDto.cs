namespace PortRiskMonitor.Application.DTOs.Read;

public record DashboardStatusDto(
    IEnumerable<KriStatusCardDto> KriCards,
    CompositeScoreDto CompositeScore,
    IEnumerable<AlertDto> ActiveAlerts,
    DateTime GeneratedAt
);
