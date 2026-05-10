using RiskMonitor.Logic;

namespace PortRiskMonitor.Application.Interfaces;

public interface IBerthOccupancyService : IKriScore<double>
{
    uint GetOccupiedCount();
    uint GetTotalCount();
    ICollection<BerthStatusDto> GetBerthDetails();
}
