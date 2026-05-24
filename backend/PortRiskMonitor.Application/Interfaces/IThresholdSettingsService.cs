using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IThresholdSettingsService
{
    Task<ThresholdSettingsDto> GetAllAsync();
    Task<ThresholdSettingsDto> UpdateAsync(ThresholdSettingsDto settings);
    Task<ThresholdSettingsDto> ResetAsync();
}
