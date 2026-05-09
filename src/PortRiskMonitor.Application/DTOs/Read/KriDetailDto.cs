namespace PortRiskMonitor.Application.DTOs.Read;

public record KriDetailDto(
    KriDefinitionDto Definition,
    KriStatusCardDto CurrentStatus,
    IEnumerable<KriReadingDto> RecentReadings, // last 30 days
    IEnumerable<AlertDto> AlertHistory      // last 10 alerts for this KRI
);
