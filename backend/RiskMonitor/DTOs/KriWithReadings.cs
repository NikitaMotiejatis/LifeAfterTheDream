using RiskMonitor.Entities;

namespace RiskMonitor.DTOs;

public record KriWithReadings
{
    public required Kri Kri { get; init; }
    public required IEnumerable<KriReading> Readings { get; init; }
};
