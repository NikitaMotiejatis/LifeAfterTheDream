namespace PortRiskMonitor.Infrastructure.VesselDelayRate;

public class VesselDelayRateRepo : IVesselDelayRateRepo
{
    private List<VesselDelayDto> _vesselDelays = generateMockData();


    public ICollection<VesselDelayDto> GetDelayDetails()
        => _vesselDelays;
    private static List<VesselDelayDto> generateMockData()
    {
        var mockData = new List<VesselDelayDto>();

        var vesselPool = new[]
        {
            ("Baltic Trader",    "Bulk"),
            ("Klaipeda Express", "Container"),
            ("Nordic Ferry",     "RoPax"),
            ("Amber Carrier",    "Bulk"),
            ("Scan Link",        "RoPax"),
            ("Baltic Feeder",    "Container"),
            ("Palanga Star",     "Tanker"),
            ("Nemunas",          "General"),
            ("Amber Sky",        "Container"),
            ("Lietava",          "Bulk"),
        };

        foreach (var (name, type) in vesselPool)
        {
            var scheduledTime = DateTime.UtcNow.AddHours(Random.Shared.Next(-12, 12));
            DateTime? actualTime = null;
            if (Random.Shared.NextDouble() > 0.5)
                actualTime = scheduledTime.AddHours(-5.0 * Random.Shared.NextDouble());

            var status = "OnTime";
            {
                var delta = scheduledTime - (actualTime is null ? DateTime.UtcNow : actualTime);
                if (delta < TimeSpan.Zero)
                    status = delta > TimeSpan.FromHours(4.0) ? "VeryLate" : "Delayed";
            }

            mockData.Add(new VesselDelayDto(
                VesselName: name,
                VesselType: type,
                ScheduledTime: scheduledTime,
                ActualTime: actualTime,
                Status: status
            ));
        }

        return mockData;
    }
}
