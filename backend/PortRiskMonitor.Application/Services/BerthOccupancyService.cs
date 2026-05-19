using System.Data;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.BerthOccupancy;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Services;

public class BerthOccupancyService : KriService, IBerthOccupancyService
{
    private readonly IBerthOccupancyRepo _berthOccupancyRepo;

    public BerthOccupancyService(IBerthOccupancyRepo berthOccupancyRepo)
        : base(berthOccupancyRepo)
    {
        _berthOccupancyRepo = berthOccupancyRepo;
    }

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
