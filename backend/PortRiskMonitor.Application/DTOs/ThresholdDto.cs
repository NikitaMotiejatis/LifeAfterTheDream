using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record ThresholdDto(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("severity")] string Severity
);
