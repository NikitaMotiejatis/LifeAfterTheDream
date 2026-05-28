namespace PortRiskMonitor.Application.DTOs;

public record NotifyRequest(string KriName, double Value, string? Message);
