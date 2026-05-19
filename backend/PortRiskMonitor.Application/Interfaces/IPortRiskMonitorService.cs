using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IPortRiskMonitorService
{
    Task<IEnumerable<KriCardDto>> GetKriCards(string preset, string? from, string? to);
}
