using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record PortStatusDto
{
    [JsonPropertyName("disruptionIndex")]
    public required double DisruptionIndex { get; init; }

    [JsonPropertyName("riskLevel")]
    public required string RiskLevel { get; init; }

    [JsonPropertyName("greenMax")]
    public required double GreenMax { get; init; }

    [JsonPropertyName("yellowMax")]
    public required double YellowMax { get; init; }

    [JsonPropertyName("sparkline")]
    public required ICollection<DataPoint> Sparkline { get; init; }
};
