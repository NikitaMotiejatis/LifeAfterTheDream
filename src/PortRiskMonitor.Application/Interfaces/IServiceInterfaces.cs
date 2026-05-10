// ============================================================
// Interfaces.cs — All Business Logic Layer service contracts
//
// These interfaces define the API surface of the Application layer.
// The API controllers depend ONLY on these interfaces — never on
// concrete service classes directly. This enables:
//   - Unit testing with mocked implementations
//   - Swapping implementations without changing controller code
// ============================================================

using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Write;

using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

// ── IKriService — main CRUD + business logic for KRI definitions ─────────────
public interface IKriService
{
    // Returns all KRIs as DTOs for the KRI Manager list
    Task<IEnumerable<KriDefinitionDto>> GetAllKrisAsync();

    // Returns one KRI with its latest reading and active alert status
    // Used by the KRI detail view
    Task<KriDetailDto?> GetKriDetailAsync(Guid id);

    // Returns the current "dashboard snapshot" — one status card per KRI
    // Used by the dashboard polling endpoint (/api/kris/status)
    Task<DashboardStatusDto> GetDashboardStatusAsync();

    // Creates a new KRI. Validates that weights still sum to ≤1.0 after addition.
    // Returns the created DTO. Throws ValidationException if invalid.
    Task<KriDefinitionDto> CreateKriAsync(CreateKriDto dto);

    // Updates an existing KRI. The DTO must include RowVersion for optimistic locking.
    // Throws DbUpdateConcurrencyException if a conflict is detected.
    // Throws NotFoundException if the KRI doesn't exist.
    Task<KriDefinitionDto> UpdateKriAsync(Guid id, UpdateKriDto dto);

    // Deletes a KRI and all its readings and alerts (cascade).
    // Throws NotFoundException if the KRI doesn't exist.
    Task DeleteKriAsync(Guid id);

    // Manual value override — allows the demo presenter to force a specific value
    // (e.g., force a red alert for a KRI). Writes a new reading with IsSimulated=false.
    Task<KriReadingDto> OverrideKriValueAsync(Guid id, double value);
}

// ── IRiskScoreEngine — calculates risk levels and composite score ─────────────
public interface IRiskScoreEngine
{
    // Determines the risk level for a single KRI value given its thresholds
    RiskLevel EvaluateRiskLevel(double value, double greenMax, double yellowMax, bool higherIsWorse);

    // Normalizes a raw KRI value to a 0-100 score for the composite calculation
    double NormalizeScore(double value, double greenMax, double yellowMax, bool higherIsWorse);

    // Calculates the weighted Composite Risk Score across all KRI readings
    // Returns a value 0-100 and an overall RiskLevel
    CompositeScoreDto CalculateCompositeScore(IEnumerable<KriScoreInputDto> inputs);
}

// ── IRiskScoreStrategy — pluggable algorithm for composite calculation ────────
// NFR: Extensibility — Strategy Pattern
// Swap the algorithm by changing appsettings.json "RiskScoring:Strategy"
// Each strategy is a new class implementing this interface — old code never changes
public interface IRiskScoreStrategy
{
    // Given a list of normalized scores (0-100) and their weights (0.0-1.0),
    // returns the final composite score (0-100)
    double Calculate(IEnumerable<KriScoreInputDto> inputs);
}

// ── IAlertService — creates and resolves threshold breach alerts ──────────────
public interface IAlertService
{
    // Returns all currently active (unresolved) alerts
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync();

    // Returns alerts for a specific KRI
    Task<IEnumerable<AlertDto>> GetAlertsForKriAsync(Guid kriId, int take = 10);

    // Evaluates a new KRI reading and creates or resolves alerts as needed.
    // Called by MockDataBackgroundService and OverrideKriValueAsync after every new reading.
    Task EvaluateAndUpdateAlertsAsync(Guid kriId, double newValue, double greenMax, double yellowMax, bool higherIsWorse);
}

// ── IReportService — generates the risk report ────────────────────────────────
public interface IReportService
{
    // Returns the full report: composite score + all KRIs in yellow/red + recent alerts
    Task<RiskReportDto> GenerateReportAsync();
}
