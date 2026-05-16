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
