namespace PortRiskMonitor.Application.DTOs; 
public record NotifyResponse(
    bool Ok,
    string Channel,
    IReadOnlyList<string> Decorators,
    IReadOnlyList<string>? Recipients,
    string? Error);