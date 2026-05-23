namespace PortRiskMonitor.Application.DTOs;

public record ThresholdPairDto(double Green, double Yellow);

// Keyed by Kri.Slug, e.g. { "berth": { green: 70, yellow: 90 }, ... }
public class ThresholdSettingsDto : Dictionary<string, ThresholdPairDto>
{
    public ThresholdSettingsDto() { }
    public ThresholdSettingsDto(IDictionary<string, ThresholdPairDto> src) : base(src) { }
}
