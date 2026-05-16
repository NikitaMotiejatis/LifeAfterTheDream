using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.CustomsDwellTime;

public class CustomsDwellTimeRepo : ICustomsDwellTimeRepo
{
    private readonly AppDbContext _db;

    public CustomsDwellTimeRepo(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Kri> GetKri()
        => _db.Kris
            .First(kri => kri.Slug == "customs");

    public async Task<IEnumerable<KriReading>> GetAllReadings()
        => _db.KriReadings
            .Where(r => r.Kri.Slug == "customs");

    public ICollection<CustomsDwellDto> GetDwellDetails(uint count, double averageDwell, string phase)
    {
        var cargoTypes = new[] { "Container", "Container", "Container", "Container", "Bulk", "Bulk", "Bulk", "Liquid", "RoRo", "RoRo" };

        var details = new List<CustomsDwellDto>();

        for (uint i = 0; i < Math.Min(count, 20); i++)
        {
            var dwellHours = Math.Max(1.0, averageDwell + (Random.Shared.NextDouble() * 20 - 10));
            var status = dwellHours > 72 ? "Overdue"
                           : phase == "Inspection" && i % 4 == 0 ? "UnderInspection"
                           : "Normal";

            details.Add(new CustomsDwellDto(
                CargoRef: $"KLJ-{DateTime.UtcNow:yyyyMMdd}-{1000 + i}",
                CargoType: cargoTypes[i % cargoTypes.Length],
                ArrivedAtCustoms: DateTime.UtcNow.AddHours(-dwellHours),
                DwellHours: dwellHours,
                Status: status
            ));
        }

        return details;
    }
}
