using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Data;

namespace PortRiskMonitor.Application.Services;

public class CustomsDwellTimeService : ICustomsDwellTimeService
{
    private const float NormalDwellHours = 18f;
    private const float InspectionAddedHours = 24f;
    private const float WeekendBacklogHours = 36f;
    private const uint NormalPendingCount = 45;
    private const uint InspectionPendingCount = 70;

    public const float GreenMax = 24f;
    public const float YellowMax = 72f;

    private static float _currentAvgDwell = NormalDwellHours;
    private static string _currentPhase = "Normal";
    private static DateTime _phaseStarted = DateTime.UtcNow;
    private static DateTime _nextPhaseChange = DateTime.UtcNow.AddHours(6);

    private readonly AppDbContext _db;

    public CustomsDwellTimeService(AppDbContext db)
    {
        _db = db;
    }

    public double GetScoreValue() => GetAverageDwellHours();

    public ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null)
        => throw new NotImplementedException("TODO: wire KRI definition ID");

    public float GetAverageDwellHours()
    {
        UpdatePhaseIfNeeded();
        return Math.Max(4f, _currentAvgDwell);
    }

    public uint GetPendingCount() => _currentPhase == "Inspection" ? InspectionPendingCount : NormalPendingCount;

    public uint GetOverdueCount()
    {
        var avg = GetAverageDwellHours();
        if (avg < 24) return 0;
        if (avg < 48) return (uint)(GetPendingCount() * 0.05f);
        if (avg < 72) return (uint)(GetPendingCount() * 0.15f);
        return (uint)(GetPendingCount() * 0.35f);
    }

    public ICollection<CustomsDwellDto> GetDwellDetails()
    {
        var count = (int)GetPendingCount();
        var avgDwell = GetAverageDwellHours();

        var cargoTypes = new[] { "Container", "Container", "Container", "Container", "Bulk", "Bulk", "Bulk", "Liquid", "RoRo", "RoRo" };

        var details = new List<CustomsDwellDto>();
        for (int i = 0; i < Math.Min(count, 20); i++)
        {
            var dwellHours = Math.Max(1f, avgDwell + (float)(Random.Shared.NextDouble() * 20 - 10));
            var status = dwellHours > 72 ? "Overdue"
                           : _currentPhase == "Inspection" && i % 4 == 0 ? "UnderInspection"
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

    private void UpdatePhaseIfNeeded()
    {
        if (DateTime.UtcNow < _nextPhaseChange) return;

        var now = DateTime.UtcNow;
        var dayOfWeek = now.DayOfWeek;
        var hour = now.Hour;

        if (dayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday && hour is >= 8 and <= 16)
            SetPhase("Inspection", NormalDwellHours + InspectionAddedHours, 8);
        else if (dayOfWeek == DayOfWeek.Friday && hour >= 16)
            SetPhase("WeekendBacklog", WeekendBacklogHours, 64);
        else if (dayOfWeek == DayOfWeek.Monday && hour < 8)
            SetPhase("WeekendBacklog", WeekendBacklogHours - (float)(now - _phaseStarted).TotalHours, 8);
        else
            SetPhase("Normal", NormalDwellHours, 4 + Random.Shared.Next(0, 4));
    }

    private static void SetPhase(string phase, float dwell, int durationHrs)
    {
        _currentPhase = phase;
        _currentAvgDwell = dwell;
        _phaseStarted = DateTime.UtcNow;
        _nextPhaseChange = DateTime.UtcNow.AddHours(durationHrs);
    }
}
