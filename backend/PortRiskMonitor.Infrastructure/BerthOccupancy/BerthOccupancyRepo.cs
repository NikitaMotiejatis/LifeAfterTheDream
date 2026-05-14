using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.BerthOccupancy;

public class BerthOccupancyRepo : IBerthOccupancyRepo
{
    private List<BerthStatusDto> _berthsStatus = generateMockData();


    public ICollection<BerthStatusDto> GetBerthDetails()
        => _berthsStatus;

    public ICollection<KriReading> GetAllReadings()
        => new List<KriReading>();

    public ICollection<Alert> GetAllAlerts()
        => new List<Alert>();


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
