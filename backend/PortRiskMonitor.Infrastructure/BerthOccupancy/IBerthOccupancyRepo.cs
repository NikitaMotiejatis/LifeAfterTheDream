using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.BerthOccupancy;

public interface IBerthOccupancyRepo : IKriRepository
{
    public ICollection<BerthStatusDto> GetBerthDetails();
}
