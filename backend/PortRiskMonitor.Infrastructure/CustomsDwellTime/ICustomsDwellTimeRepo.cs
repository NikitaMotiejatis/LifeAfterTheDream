namespace PortRiskMonitor.Infrastructure.CustomsDwellTime;

public interface ICustomsDwellTimeRepo
{
    ICollection<CustomsDwellDto> GetDwellDetails(uint count, double averageDwell, string phase);
}
