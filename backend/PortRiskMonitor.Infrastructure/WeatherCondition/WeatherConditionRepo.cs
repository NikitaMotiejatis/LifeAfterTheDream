using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.WeatherCondition;

public class WeatherConditionRepo : IWeatherConditionRepo
{
    private readonly AppDbContext _db;

    public WeatherConditionRepo(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Kri> GetKri()
        => _db.Kris
            .First(kri => kri.Slug == "weather");

    public IQueryable<KriReading> GetAllReadings()
        => _db.KriReadings
            .Where(r => r.Kri.Slug == "weather");
}
