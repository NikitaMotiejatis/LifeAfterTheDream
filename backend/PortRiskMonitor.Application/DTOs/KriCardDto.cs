using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record KriCardDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("formula")] string Formula,
    [property: JsonPropertyName("thresholds")] ThresholdDto[] Thresholds,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("greenMax")] double GreenMax,
    [property: JsonPropertyName("yellowMax")] double YellowMax,
    [property: JsonPropertyName("sparkline")] ICollection<DataPoint> Sparkline
);

