using PortRiskMonitor.Infrastructure.CustomsDwellTime;
using RiskMonitor.Logic;

namespace PortRiskMonitor.Application.Interfaces;

public interface ICustomsDwellTimeService : IKriScore<double>
{
    double GetAverageDwellHours();
    uint GetPendingCount();
    uint GetOverdueCount();
    ICollection<CustomsDwellDto> GetDwellDetails();
}
