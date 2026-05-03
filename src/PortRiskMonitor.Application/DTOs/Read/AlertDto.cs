namespace PortRiskMonitor.Application.DTOs.Read;

public record AlertDto(
    Guid         Id,
    Guid         KriDefinitionId,
    string       KriName,
    string       Level,          // "Yellow" | "Red"
    double       TriggerValue,
    DateTime     TriggeredAt,
    DateTime?    ResolvedAt,
    bool         IsActive,
    string       Message
);