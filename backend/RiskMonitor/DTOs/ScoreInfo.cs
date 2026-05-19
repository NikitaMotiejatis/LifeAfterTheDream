namespace RiskMonitor.DTOs;

public record ScoreInfo
{
    public required DateTime Timestamp { get; init; }
    public required double Value { get; init; }
}
