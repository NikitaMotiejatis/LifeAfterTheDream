// ============================================================
// ReportService.cs — Generates the risk report snapshot
// ============================================================

using Microsoft.Extensions.Configuration;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Repositories;

using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.Services;

public class ReportService : IReportService
{
    private readonly IKriRepository _kriRepo;
    private readonly IAlertRepository _alertRepo;
    private readonly IRiskScoreEngine _riskEngine;
    private readonly IConfiguration _config;

    public ReportService(
        IKriRepository kriRepo,
        IAlertRepository alertRepo,
        IRiskScoreEngine riskEngine,
        IConfiguration config)
    {
        _kriRepo = kriRepo;
        _alertRepo = alertRepo;
        _riskEngine = riskEngine;
        _config = config;
    }

    public async Task<RiskReportDto> GenerateReportAsync()
    {
        var latestReadings = await _kriRepo.GetLatestReadingsAsync();
        var allKris = await _kriRepo.GetAllAsync();
        var recentAlerts = await _alertRepo.GetAllAlertsAsync(take: 10);

        var kriDict = allKris.ToDictionary(k => k.Id);
        var readingDict = latestReadings.ToDictionary(r => r.KriDefinitionId);

        var kriCards = new List<KriStatusCardDto>();
        var scoreInputs = new List<KriScoreInputDto>();
        int greenCount = 0, yellowCount = 0, redCount = 0;
        var breachingKris = new List<KriStatusCardDto>();

        foreach (var kri in allKris)
        {
            if (readingDict.TryGetValue(kri.Id, out var reading))
            {
                var riskLevel = _riskEngine.EvaluateRiskLevel(reading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
                var normalizedScore = _riskEngine.NormalizeScore(reading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);

                var card = new KriStatusCardDto(
                    KriId: kri.Id,
                    Name: kri.Name,
                    Unit: kri.Unit,
                    CurrentValue: reading.Value,
                    NormalizedScore: normalizedScore,
                    RiskLevel: riskLevel,
                    HasActiveAlert: false, // TODO: check active alerts
                    TrendDirection: null,
                    LastUpdated: reading.Timestamp
                );

                kriCards.Add(card);
                scoreInputs.Add(new KriScoreInputDto(KriId: kri.Id, NormalizedScore: normalizedScore, Weight: kri.Weight));

                if (riskLevel == RiskLevel.Low) greenCount++;
                else if (riskLevel == RiskLevel.Medium) { yellowCount++; breachingKris.Add(card); }
                else { redCount++; breachingKris.Add(card); }
            }
            else
            {
                greenCount++; // No reading, assume green
            }
        }

        var compositeScore = _riskEngine.CalculateCompositeScore(scoreInputs);

        var alertDtos = recentAlerts.Select(a => new AlertDto(
            Id: a.Id,
            KriDefinitionId: a.KriDefinitionId,
            KriName: a.KriName,
            Level: a.Level,
            TriggerValue: a.TriggerValue,
            TriggeredAt: a.TriggeredAt,
            ResolvedAt: a.ResolvedAt,
            IsActive: a.IsActive,
            Message: a.Message
        ));

        return new RiskReportDto(
            PortName: _config["Port:Name"] ?? "Demo Port",
            GeneratedAt: DateTime.UtcNow,
            CompositeScore: compositeScore,
            GreenCount: greenCount,
            YellowCount: yellowCount,
            RedCount: redCount,
            BreachingKris: breachingKris,
            RecentAlerts: alertDtos
        );
    }
}
