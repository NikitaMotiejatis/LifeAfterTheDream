using PortRiskMonitor.Infrastructure.BerthOccupancy;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IBerthOccupancyService : IKriService
{
    uint GetOccupiedCount();
    uint GetTotalCount();
    ICollection<BerthStatusDto> GetBerthDetails();
}
