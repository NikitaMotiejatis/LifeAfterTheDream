using PortRiskMonitor.Infrastructure.CustomsDwellTime;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface ICustomsDwellTimeService : IKriService
{
    double GetAverageDwellHours();
    uint GetPendingCount();
    uint GetOverdueCount();
    ICollection<CustomsDwellDto> GetDwellDetails();
}
