using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Data;

namespace PortRiskMonitor.Application.Services;

public class VesselDelayRateService : IVesselDelayRateService
{
    private const double DelayThresholdHours = 2.0;
    private const float  NormalDelayRatePct  = 8f;

    public const float GreenMax  = 10f;
    public const float YellowMax = 25f;

    private readonly IRiskScoreEngine _riskCalculator;
    private readonly AppDbContext           _db;

    private static float    _currentDelayRate = NormalDelayRatePct;
    private static DateTime _lastUpdate       = DateTime.MinValue;

    public VesselDelayRateService(IRiskScoreEngine riskCalculator, AppDbContext db)
    {
        _riskCalculator = riskCalculator;
        _db             = db;
    }

    public float GetScoreValue() => GetDelayedCount() / (float)GetTotalCount() * 100f;

    public ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null)
    {
        // For demonstration, generate synthetic historical data for the past 24 hours
        /*
        var scores = new List<(DateTime Timestamp, double Score)>();
        var now    = DateTime.UtcNow;
        for (int i = 0; i < 24; i++)
        {
            var timestamp = now.AddHours(-i);
            var score     = (float)(NormalDelayRatePct + Math.Sin(i / 3.0) * 5 + Random.Shared.NextDouble() * 3 - 1.5);
            scores.Add((timestamp, Math.Clamp(score, 2f, 45f)));
        }
        return scores.OrderBy(s => s.Timestamp).ToList();
        */
        // TODO: Replace with real historical data retrieval from the database\
        throw new NotImplementedException("TODO: wire KRI definition ID");
    }

    public uint GetDelayedCount() => (uint)Math.Round(GetTotalCount() * GetCurrentDelayRate() / 100f);

    public uint GetTotalCount() => DateTime.UtcNow.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 30u : 45u;

    public ICollection<VesselDelayDto> GetDelayDetails()
    {
        var delayed = (int)GetDelayedCount();
        var total   = (int)GetTotalCount();

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

        var vessels = new List<VesselDelayDto>();
        for (int i = 0; i < Math.Min(total, vesselPool.Length); i++)
        {
            var (name, type) = vesselPool[i];
            var isDelayed = i < delayed;
            var delayHrs  = isDelayed
                ? DelayThresholdHours + Random.Shared.NextDouble() * 6
                : Random.Shared.NextDouble() * 1.5 - 0.5;
            var scheduled = DateTime.UtcNow.AddHours(-Random.Shared.Next(0, 12));

            vessels.Add(new VesselDelayDto(
                VesselName:    name,
                VesselType:    type,
                ScheduledTime: scheduled,
                ActualTime:    scheduled.AddHours(delayHrs),
                DelayHours:    delayHrs,
                Status:        isDelayed ? (delayHrs > 4 ? "VeryLate" : "Delayed") : "OnTime"
            ));
        }
        return vessels;
    }

    private float GetCurrentDelayRate()
    {
        if ((DateTime.UtcNow - _lastUpdate).TotalMinutes < 1)
            return _currentDelayRate;

        var delta = (float)(Random.Shared.NextDouble() * 4 - 2);
        _currentDelayRate = Math.Clamp(_currentDelayRate + delta, 2f, 45f);
        _lastUpdate       = DateTime.UtcNow;
        return _currentDelayRate;
    }

    double IIndicatorScore.GetScoreValue() => GetScoreValue();
    ICollection<(DateTime Timestamp, double Score)> IIndicatorScore.GetScores(DateTime? from, DateTime? to) => GetScores(from, to);
    uint IVesselDelayRateService.GetDelayedCount() => GetDelayedCount();
    uint IVesselDelayRateService.GetTotalCount() => GetTotalCount();
    ICollection<VesselDelayDto> IVesselDelayRateService.GetDelayDetails() => GetDelayDetails();
}
