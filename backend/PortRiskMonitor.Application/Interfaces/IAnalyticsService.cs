using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetAnalytics(string slug, string? from, string? to);
}
