using PortRiskMonitor.Application.DTOs;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IPortStatusService : IKriService
{
    Task<PortStatusDto> GetPortStatus(string preset, string? fromStr, string? toStr);
    Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame);
}
