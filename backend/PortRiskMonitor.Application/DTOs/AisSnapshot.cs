namespace PortRiskMonitor.Application.DTOs;

public record AisSnapshot
{
    public required object Targets { get; init; }
};
