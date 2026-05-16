using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.BerthOccupancy;

public class BerthOccupancyRepo : IBerthOccupancyRepo
{
    private readonly AppDbContext _db;

    public BerthOccupancyRepo(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Kri> GetKri()
        => _db.Kris
            .First(kri => kri.Slug == "berth");

    public async Task<IEnumerable<KriReading>> GetAllReadings()
        => _db.KriReadings
            .Where(r => r.Kri.Slug == "berth");

    private List<BerthStatusDto> _berthsStatus = generateMockData();

    public ICollection<BerthStatusDto> GetBerthDetails()
        => _berthsStatus;
    private static List<BerthStatusDto> generateMockData()
    {
        var mockData = new List<BerthStatusDto>();

        var occupied = (int)10;
        var VesselTypes = new string[]
        {
            "Container", "Container", "Container",
            "Bulk",      "Bulk",      "Bulk",
            "Tanker",    "Tanker",
            "RoRo",      "RoRo",
            "Ferry",
            "General"
        };

        for (int i = 0; i < 30; i++)
        {
            var isOccupied = i < occupied;
            mockData.Add(new BerthStatusDto(
                BerthId: $"B-{101 + i}",
                IsOccupied: isOccupied,
                VesselType: isOccupied ? VesselTypes[i % VesselTypes.Length] : "Empty",
                OccupiedSince: isOccupied ? DateTime.UtcNow.AddHours(-Random.Shared.Next(1, 24)) : null
            ));
        }

        return mockData;
    }
}
