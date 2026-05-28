using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Data.Alerts;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Services;

public class AlertingService : IAlertingService
{
    private readonly IRiskMonitorRepository _riskRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IAlertNotifier _notifier;
    private readonly ILogger<AlertingService> _logger;

    public AlertingService(
        IRiskMonitorRepository riskRepo,
        IAlertRepository alertRepo,
        IAlertNotifier notifier,
        ILogger<AlertingService> logger)
    {
        _riskRepo = riskRepo;
        _alertRepo = alertRepo;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task EvaluateAllLatestAsync(CancellationToken cancellationToken = default)
    {
        var latestReadings = await _riskRepo.GetLatestReadings().ToListAsync();

        foreach (var reading in latestReadings)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EvaluateAsync(reading, cancellationToken);
        }
    }

    private async Task EvaluateAsync(KriReading reading, CancellationToken cancellationToken)
    {
        var kri = reading.Kri;
        if (kri is null) return;

        var level = Classify(reading.Value, kri.GreenMax, kri.YellowMax);
        var activeAlerts = await _alertRepo.GetActiveAlertsForKri(kri.Id).ToListAsync();

        if (level == "Green")
        {
            foreach (var active in activeAlerts)
                await _alertRepo.ResolveAsync(active.Id);
            return;
        }

        // Non-green: ensure exactly one active alert at the current level.
        // Resolve any active alert at a different level (e.g. Yellow→Red escalation).
        var matching = activeAlerts.FirstOrDefault(a => a.Level == level);
        var stale = activeAlerts.Where(a => a.Level != level);
        foreach (var s in stale) await _alertRepo.ResolveAsync(s.Id);

        if (matching is not null) return; // already alerted at this level — dedup

        var alert = await _alertRepo.CreateAsync(new Alert
        {
            KriId = kri.Id,
            KriName = kri.Name,
            Level = level,
            TriggerValue = reading.Value,
            Message = $"{kri.Name} entered {level.ToLowerInvariant()} zone at {reading.Value}{kri.Unit}",
        });

        if (level == "Red")
        {
            try
            {
                await _notifier.NotifyRedAsync(alert, cancellationToken);
            }
            catch (Exception ex)
            {
                // Notifier failure must not block alert persistence.
                _logger.LogError(ex, "Alert notifier failed for KRI {Slug}", kri.Slug);
            }
        }
    }

    private static string Classify(double value, double greenMax, double yellowMax)
    {
        if (value <= greenMax) return "Green";
        if (value <= yellowMax) return "Yellow";
        return "Red";
    }
}
