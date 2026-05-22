using System.Text.Json.Serialization;

namespace PortRiskMonitor.Application.DTOs;

public record AnalyticsDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("greenMax")] double GreenMax,
    [property: JsonPropertyName("yellowMax")] double YellowMax,
    [property: JsonPropertyName("sparkline")] ICollection<DataPoint> Sparkline
);

