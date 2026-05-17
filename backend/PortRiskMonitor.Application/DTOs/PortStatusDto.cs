using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record PortStatusDto
{
    [JsonPropertyName("disruptionIndex")]
    public required double DisruptionIndex { get; init; }

    [JsonPropertyName("riskLevel")]
    public required string RiskLevel { get; init; }

    [JsonPropertyName("sparkline")]
    public required ICollection<SparkPoint> Sparkline { get; init; }

    public record SparkPoint
    {
        [JsonPropertyName("label")]
        public required string Label { get; init; }

        [JsonPropertyName("value")]
        public required double Value { get; init; }
    }
};
