using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.Application.Services;

public class PortStatusService : IPortStatusService
{
    public ICollection<(string hour, double value)> GetTrend(string trendTimeFrame)
    {
        return new List<(string hour, double value)>();
    }
}
