using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record DataPoint(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("value")] double Value
);

public record KriCardDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("value")] string Value,
    //[property: JsonPropertyName("formula")] string Formula,
    //[property: JsonPropertyName("thresholds")] ThresholdDto[] Thresholds,
    [property: JsonPropertyName("sparkline")] ICollection<DataPoint> Sparkline
);

