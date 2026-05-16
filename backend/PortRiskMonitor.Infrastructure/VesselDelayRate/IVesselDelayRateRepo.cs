namespace PortRiskMonitor.Infrastructure.VesselDelayRate;

public interface IVesselDelayRateRepo
{
    public ICollection<VesselDelayDto> GetDelayDetails();
}
