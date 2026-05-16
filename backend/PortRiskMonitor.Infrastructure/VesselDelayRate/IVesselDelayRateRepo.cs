using RiskMonitor.Repositories;

namespace PortRiskMonitor.Infrastructure.VesselDelayRate;

public interface IVesselDelayRateRepo : IKriRepository
{
    public ICollection<VesselDelayDto> GetDelayDetails();
}
