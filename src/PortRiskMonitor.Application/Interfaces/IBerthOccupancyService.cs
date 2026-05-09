using System.Collections.Generic;

namespace PortRiskMonitor.Application.Interfaces;

public interface IBerthOccupancyService : IIndicatorScore
{
    uint GetOccupiedCount();
    uint GetTotalCount();
    ICollection<BerthStatusDto> GetBerthDetails();
}
