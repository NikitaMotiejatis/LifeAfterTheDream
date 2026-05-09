using System.Collections.Generic;
using PortRiskMonitor.Application.DTOs.Read;

namespace PortRiskMonitor.Application.Interfaces;

public interface IVesselDelayRateService : IIndicatorScore
{
    uint GetDelayedCount();
    uint GetTotalCount();
    ICollection<VesselDelayDto> GetDelayDetails();
}
