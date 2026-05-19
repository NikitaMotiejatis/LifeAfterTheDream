using PortRiskMonitor.Infrastructure.VesselDelayRate;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Interfaces;

public interface IVesselDelayRateService : IKriService
{
    uint GetDelayedCount();
    uint GetTotalCount();
    ICollection<VesselDelayDto> GetDelayDetails();
}
