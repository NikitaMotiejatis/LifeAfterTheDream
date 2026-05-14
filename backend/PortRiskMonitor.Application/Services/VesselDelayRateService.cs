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

    public ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null)
        => _vesselDelayRateRepo
            .GetAllReadings()
            .Where(details => (from ?? DateTime.MinValue) <= details.MeasuredAt && details.MeasuredAt <= (to ?? DateTime.MaxValue))
            .Select(details => (Timestamp: details.MeasuredAt, Score: details.Value))
            .ToArray();

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
