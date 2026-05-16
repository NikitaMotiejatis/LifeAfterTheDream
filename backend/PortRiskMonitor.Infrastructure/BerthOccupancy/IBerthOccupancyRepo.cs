namespace PortRiskMonitor.Infrastructure.BerthOccupancy;

public interface IBerthOccupancyRepo
{
    public ICollection<BerthStatusDto> GetBerthDetails();
}
