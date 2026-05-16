namespace PortRiskMonitor.Application.Interfaces;

public interface IPortStatusService
{
    ICollection<(string hour, double value)> GetTrend(string trendTimeFrame);
}
