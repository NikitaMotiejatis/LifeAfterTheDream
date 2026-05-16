using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.CustomsDwellTime;

public interface ICustomsDwellTimeRepo : IKriRepository
{
    ICollection<CustomsDwellDto> GetDwellDetails(uint count, double averageDwell, string phase);
}
