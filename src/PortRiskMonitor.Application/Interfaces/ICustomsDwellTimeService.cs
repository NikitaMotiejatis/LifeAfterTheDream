using PortRiskMonitor.Application.DTOs.Enums;
using RiskMonitor.Logic;

namespace PortRiskMonitor.Application.Interfaces;

public interface ICustomsDwellTimeService : IKriScore<double>
{
    float GetAverageDwellHours();
    uint GetPendingCount();
    uint GetOverdueCount();
    ICollection<CustomsDwellDto> GetDwellDetails();
}
