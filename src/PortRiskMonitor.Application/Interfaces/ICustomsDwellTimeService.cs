using System.Collections.Generic;
using PortRiskMonitor.Application.DTOs.Enums;

namespace PortRiskMonitor.Application.Interfaces;

public interface ICustomsDwellTimeService : IIndicatorScore
{
    float GetAverageDwellHours();
    uint GetPendingCount();
    uint GetOverdueCount();
    ICollection<CustomsDwellDto> GetDwellDetails();
}
