using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record DataPoint
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("value")]
    public required double Value { get; init; }
}

