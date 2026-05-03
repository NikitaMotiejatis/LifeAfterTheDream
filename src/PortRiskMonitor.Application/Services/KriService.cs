// ============================================================
// KriService.cs — Business Logic for KRI CRUD and dashboard
//
// This is the most important service in the system.
// It orchestrates between the repository (data) and the
// risk engine (calculations) to serve the API controllers.
//
// NFR: Memory Management — registered as Scoped (per HTTP request)
// NFR: Data Access — no transaction spans across calls
// NFR: Concurrency — does NOT store state in fields; all state
//      is fetched fresh from the DB on each method call
// ============================================================

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Write;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Entities;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.Application.Services;

public class KriService : IKriService
{
    private readonly IKriRepository    _kriRepo;
    private readonly IAlertService     _alertService;
    private readonly IRiskScoreEngine  _riskEngine;

    public KriService(
        IKriRepository   kriRepo,
        IAlertService    alertService,
        IRiskScoreEngine riskEngine)
    {
        _kriRepo      = kriRepo;
        _alertService = alertService;
        _riskEngine   = riskEngine;
    }

    public async Task<IEnumerable<KriDefinitionDto>> GetAllKrisAsync()
    {
        var kris = await _kriRepo.GetAllAsync();

        return kris.Select(MapToKriDefinitionDto);
    }

    public async Task<KriDetailDto?> GetKriDetailAsync(Guid id)
    {
        var kri = await _kriRepo.GetByIdAsync(id);
        if (kri == null) return null;

        // Get recent readings (last 30 days)
        var from = DateTime.UtcNow.AddDays(-30);
        var recentReadings = await _kriRepo.GetReadingsAsync(id, from, DateTime.UtcNow);

        // Get recent alerts (last 10)
        var alertHistory = await _alertService.GetAlertsForKriAsync(id, 10);

        // Get current status
        var latestReading = recentReadings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
        if (latestReading == null)
        {
            // No readings yet, return with empty status
            var definition = MapToKriDefinitionDto(kri);
            var emptyStatus = new KriStatusCardDto(
                KriId: id,
                Name: kri.Name,
                Unit: kri.Unit,
                CurrentValue: 0,
                NormalizedScore: 0,
                RiskLevel: RiskLevelDto.Green,
                HasActiveAlert: false,
                TrendDirection: null,
                LastUpdated: DateTime.MinValue
            );
            return new KriDetailDto(
                Definition: definition,
                CurrentStatus: emptyStatus,
                RecentReadings: recentReadings.Select(MapToKriReadingDto),
                AlertHistory: alertHistory
            );
        }

        // Calculate current status
        var riskLevel = _riskEngine.EvaluateRiskLevel(latestReading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
        var normalizedScore = _riskEngine.NormalizeScore(latestReading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
        var hasActiveAlert = alertHistory.Any(a => a.IsActive);

        // Trend direction: compare with previous reading
        string? trendDirection = null;
        var previousReading = recentReadings.OrderByDescending(r => r.Timestamp).Skip(1).FirstOrDefault();
        if (previousReading != null)
        {
            var prevNormalized = _riskEngine.NormalizeScore(previousReading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
            if (normalizedScore < prevNormalized - 2) trendDirection = "Improving";
            else if (normalizedScore > prevNormalized + 2) trendDirection = "Worsening";
            else trendDirection = "Stable";
        }

        var currentStatus = new KriStatusCardDto(
            KriId: id,
            Name: kri.Name,
            Unit: kri.Unit,
            CurrentValue: latestReading.Value,
            NormalizedScore: normalizedScore,
            RiskLevel: riskLevel,
            HasActiveAlert: hasActiveAlert,
            TrendDirection: trendDirection,
            LastUpdated: latestReading.Timestamp
        );

        return new KriDetailDto(
            Definition: MapToKriDefinitionDto(kri),
            CurrentStatus: currentStatus,
            RecentReadings: recentReadings.Select(MapToKriReadingDto),
            AlertHistory: alertHistory
        );
    }

    public async Task<DashboardStatusDto> GetDashboardStatusAsync()
    {
        var kris = await _kriRepo.GetAllAsync();
        var latestReadings = await _kriRepo.GetLatestReadingsAsync();
        var activeAlerts = await _alertService.GetActiveAlertsAsync();

        // Group readings by KriId for easy lookup
        var readingsByKri = latestReadings.ToDictionary(r => r.KriDefinitionId);

        var kriCards = new List<KriStatusCardDto>();
        var scoreInputs = new List<KriScoreInputDto>();

        foreach (var kri in kris)
        {
            if (readingsByKri.TryGetValue(kri.Id, out var reading))
            {
                var riskLevel = _riskEngine.EvaluateRiskLevel(reading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
                var normalizedScore = _riskEngine.NormalizeScore(reading.Value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
                var hasActiveAlert = activeAlerts.Any(a => a.KriDefinitionId == kri.Id && a.IsActive);

                // Trend direction: need previous reading, but GetLatestReadingsAsync only gets latest per KRI
                // For simplicity, set to null or fetch more readings
                string? trendDirection = null; // TODO: Implement trend if needed

                var card = new KriStatusCardDto(
                    KriId: kri.Id,
                    Name: kri.Name,
                    Unit: kri.Unit,
                    CurrentValue: reading.Value,
                    NormalizedScore: normalizedScore,
                    RiskLevel: riskLevel,
                    HasActiveAlert: hasActiveAlert,
                    TrendDirection: trendDirection,
                    LastUpdated: reading.Timestamp
                );
                kriCards.Add(card);
                scoreInputs.Add(new KriScoreInputDto(KriId: kri.Id, NormalizedScore: normalizedScore, Weight: kri.Weight));
            }
            else
            {
                // No reading yet
                var card = new KriStatusCardDto(
                    KriId: kri.Id,
                    Name: kri.Name,
                    Unit: kri.Unit,
                    CurrentValue: 0,
                    NormalizedScore: 0,
                    RiskLevel: RiskLevelDto.Green,
                    HasActiveAlert: false,
                    TrendDirection: null,
                    LastUpdated: DateTime.MinValue
                );
                kriCards.Add(card);
                scoreInputs.Add(new KriScoreInputDto(KriId: kri.Id, NormalizedScore: 0, Weight: kri.Weight));
            }
        }

        var compositeScore = _riskEngine.CalculateCompositeScore(scoreInputs);

        return new DashboardStatusDto(
            KriCards: kriCards,
            CompositeScore: compositeScore,
            ActiveAlerts: activeAlerts,
            GeneratedAt: DateTime.UtcNow
        );
    }

    public async Task<KriDefinitionDto> CreateKriAsync(CreateKriDto dto)
    {
        // TODO: Validate that the new KRI's weight won't push total above 1.0
        var existingKris = await _kriRepo.GetAllAsync();
        var totalWeight = existingKris.Sum(k => k.Weight) + dto.Weight;
        if (totalWeight > 1.01) throw new InvalidOperationException("Total KRI weights exceed 1.0");

        var entity = new KriDefinition
        {
            Name = dto.Name,
            Unit = dto.Unit,
            Description = dto.Description,
            FormulaLabel = dto.FormulaLabel,
            GreenMax = dto.GreenMax,
            YellowMax = dto.YellowMax,
            Weight = dto.Weight,
            HigherIsWorse = dto.HigherIsWorse,
            MockBaseline = dto.MockBaseline,
            MockVariance = dto.MockVariance,
            MockPattern = dto.MockPattern
        };

        var created = await _kriRepo.CreateAsync(entity);
        return MapToKriDefinitionDto(created);
    }

    public async Task<KriDefinitionDto> UpdateKriAsync(Guid id, UpdateKriDto dto)
    {
        var existing = await _kriRepo.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"KRI with id {id} not found");

        // Apply RowVersion for optimistic locking
        existing.RowVersion = Convert.FromBase64String(dto.RowVersion);

        // Update fields
        existing.Name = dto.Name;
        existing.Unit = dto.Unit;
        existing.Description = dto.Description;
        existing.FormulaLabel = dto.FormulaLabel;
        existing.GreenMax = dto.GreenMax;
        existing.YellowMax = dto.YellowMax;
        existing.Weight = dto.Weight;
        existing.HigherIsWorse = dto.HigherIsWorse;
        existing.MockBaseline = dto.MockBaseline;
        existing.MockVariance = dto.MockVariance;
        existing.MockPattern = dto.MockPattern;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _kriRepo.UpdateAsync(existing);
        return MapToKriDefinitionDto(updated);
    }

    public async Task DeleteKriAsync(Guid id)
    {
        var existing = await _kriRepo.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"KRI with id {id} not found");

        await _kriRepo.DeleteAsync(id);
        // Cascade delete in DB removes all Readings and Alerts automatically
    }

    public async Task<KriReadingDto> OverrideKriValueAsync(Guid id, double value)
    {
        var kri = await _kriRepo.GetByIdAsync(id);
        if (kri == null) throw new KeyNotFoundException($"KRI with id {id} not found");

        var level = _riskEngine.EvaluateRiskLevel(value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);
        var score = _riskEngine.NormalizeScore(value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);

        var reading = new KriReading
        {
            KriDefinitionId = id,
            Value = value,
            NormalizedScore = score,
            RiskLevel = level.ToString(),
            Timestamp = DateTime.UtcNow,
            IsSimulated = false // Manual override
        };

        var saved = await _kriRepo.AddReadingAsync(reading);

        // Evaluate alerts
        await _alertService.EvaluateAndUpdateAlertsAsync(id, value, kri.GreenMax, kri.YellowMax, kri.HigherIsWorse);

        return MapToKriReadingDto(saved);
    }

    // ── Private mapping helpers ───────────────────────────────────────────────
    // TODO: Implement these mapping methods (or replace with AutoMapper)

    private static KriDefinitionDto MapToKriDefinitionDto(KriDefinition entity)
    {
        return new KriDefinitionDto(
            Id: entity.Id,
            Name: entity.Name,
            Unit: entity.Unit,
            Description: entity.Description,
            FormulaLabel: entity.FormulaLabel,
            GreenMax: entity.GreenMax,
            YellowMax: entity.YellowMax,
            Weight: entity.Weight,
            HigherIsWorse: entity.HigherIsWorse,
            MockBaseline: entity.MockBaseline,
            MockVariance: entity.MockVariance,
            MockPattern: entity.MockPattern,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt,
            RowVersion: Convert.ToBase64String(entity.RowVersion)
        );
    }

    private static KriReadingDto MapToKriReadingDto(KriReading entity)
    {
        return new KriReadingDto(
            Id: entity.Id,
            KriDefinitionId: entity.KriDefinitionId,
            Value: entity.Value,
            NormalizedScore: entity.NormalizedScore,
            RiskLevel: Enum.Parse<RiskLevelDto>(entity.RiskLevel),
            Timestamp: entity.Timestamp,
            IsSimulated: entity.IsSimulated
        );
    }
}
