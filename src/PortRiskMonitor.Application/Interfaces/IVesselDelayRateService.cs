using PortRiskMonitor.Infrastructure.VesselDelayRate;
using RiskMonitor.Logic;

namespace PortRiskMonitor.Application.Interfaces;

public interface IVesselDelayRateService : IKriScore<double>
{
    uint GetDelayedCount();
    uint GetTotalCount();
    ICollection<VesselDelayDto> GetDelayDetails();
}
