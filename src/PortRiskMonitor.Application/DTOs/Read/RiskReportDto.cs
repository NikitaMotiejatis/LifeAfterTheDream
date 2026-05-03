namespace PortRiskMonitor.Application.DTOs.Read;

public record RiskReportDto(
    string                     PortName,          // from appsettings.json
    DateTime                   GeneratedAt,
    CompositeScoreDto          CompositeScore,
    int                        GreenCount,
    int                        YellowCount,
    int                        RedCount,
    IEnumerable<KriStatusCardDto> BreachingKris,  // only yellow and red
    IEnumerable<AlertDto>      RecentAlerts       // last 10 alerts across all KRIs
);