using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IThresholdSettingsService
{
    Task<ThresholdSettingsDto> GetAllAsync();
    Task<ThresholdSettingsDto> UpdateAsync(ThresholdSettingsDto settings, bool force = false);
    Task<ThresholdPairDto> UpdateOneAsync(string slug, ThresholdPairDto pair);
    Task<ThresholdSettingsDto> ResetAsync();
}
