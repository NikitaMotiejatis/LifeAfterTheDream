using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.CustomsDwellTime;

namespace PortRiskMonitor.Application.Services;

public class CustomsDwellTimeService : ICustomsDwellTimeService
{
    private const double NormalDwellHours = 18.0;
    private const double InspectionAddedHours = 24.0;
    private const double WeekendBacklogHours = 36.0;
    private const uint NormalPendingCount = 45;
    private const uint InspectionPendingCount = 70;

    public const double GreenMax = 24f;
    public const double YellowMax = 72f;

    private static double _currentAvgDwell = NormalDwellHours;
    private static string _currentPhase = "Normal";
    private static DateTime _phaseStarted = DateTime.UtcNow;
    private static DateTime _nextPhaseChange = DateTime.UtcNow.AddHours(6);

    private ICustomsDwellTimeRepo _customsDwellTimeRepo;

    public CustomsDwellTimeService(ICustomsDwellTimeRepo customsDwellTimeRepo)
    {
        _customsDwellTimeRepo = customsDwellTimeRepo;
    }

    public double GetScoreValue()
    {
        UpdatePhaseIfNeeded();
        var avgDwell = GetAverageDwellHours();
        var pending = GetPendingCount();
        var overdue = GetOverdueCount();
        return Math.Min(100.0, (avgDwell / 72.0) * 50 + (pending / 100.0) * 30 + (overdue / 50.0) * 20);
    }

    public double GetAverageDwellHours()
    {
        UpdatePhaseIfNeeded();
        return Math.Max(4f, _currentAvgDwell);
    }

    public uint GetPendingCount()
        => _currentPhase == "Inspection"
            ? InspectionPendingCount
            : NormalPendingCount;

    public uint GetOverdueCount()
    {
        var avg = GetAverageDwellHours();
        if (avg < 24) return 0;
        if (avg < 48) return (uint)(GetPendingCount() * 0.05);
        if (avg < 72) return (uint)(GetPendingCount() * 0.15);
        return (uint)(GetPendingCount() * 0.35f);
    }

    public ICollection<CustomsDwellDto> GetDwellDetails()
        => _customsDwellTimeRepo.GetDwellDetails(
                GetPendingCount(),
                GetAverageDwellHours(),
                _currentPhase
            );

    private void UpdatePhaseIfNeeded()
    {
        if (DateTime.UtcNow < _nextPhaseChange)
            return;

        var now = DateTime.UtcNow;
        var dayOfWeek = now.DayOfWeek;
        var hour = now.Hour;

        if (dayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday && hour is >= 8 and <= 16)
            SetPhase("Inspection", NormalDwellHours + InspectionAddedHours, 8);
        else if (dayOfWeek == DayOfWeek.Friday && hour >= 16)
            SetPhase("WeekendBacklog", WeekendBacklogHours, 64);
        else if (dayOfWeek == DayOfWeek.Monday && hour < 8)
            SetPhase("WeekendBacklog", WeekendBacklogHours - (now - _phaseStarted).TotalHours, 8);
        else
            SetPhase("Normal", NormalDwellHours, 4 + Random.Shared.Next(0, 4));
    }

    private static void SetPhase(string phase, double dwell, int durationHrs)
    {
        _currentPhase = phase;
        _currentAvgDwell = dwell;
        _phaseStarted = DateTime.UtcNow;
        _nextPhaseChange = DateTime.UtcNow.AddHours(durationHrs);
    }
}
