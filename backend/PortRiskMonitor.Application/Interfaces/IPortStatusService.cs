using PortRiskMonitor.Application.DTOs;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IPortStatusService : IKriService
{
    Task<PortStatusDto> GetPortStatus(string preset, string? from, string? to);
    Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame);
}
