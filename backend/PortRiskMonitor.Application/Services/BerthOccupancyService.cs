using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.BerthOccupancy;

namespace PortRiskMonitor.Application.Services;

public class BerthOccupancyService : IBerthOccupancyService
{
    private IBerthOccupancyRepo _berthOccupancyRepo;

    public BerthOccupancyService(IBerthOccupancyRepo berthOccupancyRepo)
    {
        _berthOccupancyRepo = berthOccupancyRepo;
    }

    public double GetScoreValue()
        => 100.0 * (double)GetOccupiedCount() / (double)GetTotalCount();

    public ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null)
        => _berthOccupancyRepo
            .GetAllReadings()
            .Where(details => (from ?? DateTime.MinValue) <= details.MeasuredAt && details.MeasuredAt <= (to ?? DateTime.MaxValue))
            .Select(details => (Timestamp: details.MeasuredAt, Score: details.Value))
            .ToArray();

    public uint GetOccupiedCount()
        => (uint)_berthOccupancyRepo
            .GetBerthDetails()
            .Where(details => details.IsOccupied)
            .Count();

    public uint GetTotalCount()
        => (uint)_berthOccupancyRepo
            .GetBerthDetails()
            .Count();

    public ICollection<BerthStatusDto> GetBerthDetails()
        => _berthOccupancyRepo.GetBerthDetails();
}
