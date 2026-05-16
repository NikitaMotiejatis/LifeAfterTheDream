using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.VesselDelayRate;

namespace PortRiskMonitor.Application.Services;

public class VesselDelayRateService : IVesselDelayRateService
{
    private readonly IVesselDelayRateRepo _vesselDelayRateRepo;

    public VesselDelayRateService(IVesselDelayRateRepo vesselDelayRateRepo)
    {
        _vesselDelayRateRepo = vesselDelayRateRepo;
    }

    public double GetScoreValue() => 100.0 * (double)GetDelayedCount() / (double)GetTotalCount();

    public uint GetDelayedCount()
        => (uint)_vesselDelayRateRepo
            .GetDelayDetails()
            .Where(details => details.Status != "OnTime")
            .Count();

    public uint GetTotalCount()
        => (uint)_vesselDelayRateRepo
            .GetDelayDetails()
            .Count();

    public ICollection<VesselDelayDto> GetDelayDetails()
        => _vesselDelayRateRepo.GetDelayDetails();
}
