using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Data;

namespace PortRiskMonitor.Application.Services;

public class BerthOccupancyService : IBerthOccupancyService
{
    private const uint  TotalBerths        = 30;
    private const float NormalOccupancyPct = 62f;
    private const float AmplitudePct       = 15f;
    private const int   MorningPeakHourUtc = 6;
    private const int   EveningPeakHourUtc = 18;

    public const float GreenMax  = 70f;
    public const float YellowMax = 90f;

    private static readonly string[] VesselTypes =
    [
        "Container", "Container", "Container",
        "Bulk",      "Bulk",      "Bulk",
        "Tanker",    "Tanker",
        "RoRo",      "RoRo",
        "Ferry",
        "General"
    ];

    private readonly AppDbContext           _db;

    public BerthOccupancyService(AppDbContext db)
    {
        _db             = db;
    }

    public float GetScoreValue() =>
        GetOccupiedCount() / (float)GetTotalCount() * 100f;

    public ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null)
        => throw new NotImplementedException("TODO: wire KRI definition ID");

    public uint GetOccupiedCount()
    {
        var hour         = DateTime.UtcNow.Hour;
        var morningWave  = Math.Sin(2 * Math.PI * (hour - MorningPeakHourUtc) / 24.0);
        var eveningWave  = Math.Sin(2 * Math.PI * (hour - EveningPeakHourUtc) / 24.0);
        var combinedWave = (morningWave + eveningWave) / 2.0;

        var occupied = (uint)Math.Round((NormalOccupancyPct + AmplitudePct * (float)combinedWave) / 100f * TotalBerths);
        var noise    = Random.Shared.Next(-2, 3);
        return (uint)Math.Clamp((int)occupied + noise, 0, (int)TotalBerths);
    }

    public uint GetTotalCount() => TotalBerths;

    public ICollection<BerthStatusDto> GetBerthDetails()
    {
        var occupied = (int)GetOccupiedCount();
        var berths   = new List<BerthStatusDto>();

        for (int i = 0; i < TotalBerths; i++)
        {
            var isOccupied = i < occupied;
            berths.Add(new BerthStatusDto(
                BerthId:       $"B-{101 + i}",
                IsOccupied:    isOccupied,
                VesselType:    isOccupied ? VesselTypes[i % VesselTypes.Length] : "Empty",
                OccupiedSince: isOccupied ? DateTime.UtcNow.AddHours(-Random.Shared.Next(1, 24)) : null
            ));
        }

        return berths;
    }

    uint IBerthOccupancyService.GetOccupiedCount() => GetOccupiedCount();
    uint IBerthOccupancyService.GetTotalCount()    => GetTotalCount();
    ICollection<BerthStatusDto> IBerthOccupancyService.GetBerthDetails() => GetBerthDetails();

    double IIndicatorScore.GetScoreValue() => GetScoreValue();
    ICollection<(DateTime Timestamp, double Score)> IIndicatorScore.GetScores(DateTime? from, DateTime? to) => GetScores(from, to);
}
