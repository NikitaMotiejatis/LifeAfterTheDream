// ============================================================
// AlertService.cs — Creates and resolves threshold breach alerts
// MockDataBackgroundService.cs — Simulates live KRI data
// ============================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Write;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Entities;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.Application.Services;

// ────────────────────────────────────────────────────────────
// AlertService
// ────────────────────────────────────────────────────────────
public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;
    private readonly IRiskScoreEngine _riskEngine;

    public AlertService(IAlertRepository alertRepo, IRiskScoreEngine riskEngine)
    {
        _alertRepo = alertRepo;
        _riskEngine = riskEngine;
    }

    public async Task<IEnumerable<AlertDto>> GetActiveAlertsAsync()
    {
        var alerts = await _alertRepo.GetActiveAlertsAsync();

        return alerts.Select(MapToAlertDto);
    }

    public async Task<IEnumerable<AlertDto>> GetAlertsForKriAsync(Guid kriId, int take = 10)
    {
        var alerts = await _alertRepo.GetAlertsForKriAsync(kriId, take);

        return alerts.Select(MapToAlertDto);
    }

    private static AlertDto MapToAlertDto(Alert entity)
    {
        return new AlertDto(
            Id: entity.Id,
            KriDefinitionId: entity.KriDefinitionId,
            KriName: entity.KriName,
            Level: entity.Level,
            TriggerValue: entity.TriggerValue,
            TriggeredAt: entity.TriggeredAt,
            ResolvedAt: entity.ResolvedAt,
            IsActive: entity.IsActive,
            Message: entity.Message
        );
    }

    public async Task EvaluateAndUpdateAlertsAsync(
        Guid kriId,
        double newValue,
        double greenMax,
        double yellowMax,
        bool higherIsWorse)
    {
        // TODO: Implement alert evaluation logic:
        //
        // STEP 1: Determine the new risk level for this value
        //   var newLevel = _riskEngine.EvaluateRiskLevel(newValue, greenMax, yellowMax, higherIsWorse);
        //
        // STEP 2: Check if there's already an active alert at the same or higher level
        //   This prevents duplicate alerts when the KRI stays in the same band.
        //
        // STEP 3: If newLevel > existing active alert level → create a new alert
        //   (e.g., KRI moved from Yellow to Red — create a Red alert)
        //
        // STEP 4: If newLevel < existing active alert level → resolve the existing alert
        //   (e.g., KRI moved from Red back to Yellow — resolve the Red alert)
        //   Optionally create a new Yellow alert.
        //
        // STEP 5: If newLevel == Green and any active alert exists → resolve all alerts
        //
        // This logic ensures:
        //   - Only one active alert per KRI per level at a time
        //   - Alerts are automatically resolved when the KRI recovers
        //   - Level escalation (Yellow → Red) is captured as a new alert

        throw new NotImplementedException("TODO: Implement EvaluateAndUpdateAlertsAsync");
    }
}
