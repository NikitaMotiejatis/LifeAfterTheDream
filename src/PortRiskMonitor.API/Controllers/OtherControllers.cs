// ============================================================
// AlertsController.cs — Active alerts API
// ReportsController.cs — Risk report generation
// ScenariosController.cs — Demo scenario control panel
// ============================================================

using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Write;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Application.Services;

namespace PortRiskMonitor.API.Controllers;

// ────────────────────────────────────────────────────────────
// GET /api/alerts
// ────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    // GET /api/alerts/active
    // Returns all currently active (unresolved) alerts
    // Used by the dashboard alert panel and the KRI card badge counts
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<AlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var alerts = await _alertService.GetActiveAlertsAsync();
        return Ok(alerts);
    }
}

// ────────────────────────────────────────────────────────────
// GET /api/reports
// ────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/reports
    // Generates the full risk report snapshot:
    //   - Composite score at this moment
    //   - Count of KRIs by risk level (green/yellow/red)
    //   - All KRIs currently in yellow or red band
    //   - Last 10 alerts across all KRIs
    // This is what the React Reports screen renders and optionally prints.
    [HttpGet]
    [ProducesResponseType(typeof(RiskReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport()
    {
        var report = await _reportService.GenerateReportAsync();
        return Ok(report);
    }
}

// ────────────────────────────────────────────────────────────
// POST /api/scenarios — Demo control panel
// Only available in Development environment
// ────────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScenariosController : ControllerBase
{
    private readonly MockDataBackgroundService _mockService;
    private readonly IWebHostEnvironment       _env;
    private readonly ILogger<ScenariosController> _logger;

    public ScenariosController(
        MockDataBackgroundService     mockService,
        IWebHostEnvironment           env,
        ILogger<ScenariosController>  logger)
    {
        _mockService = mockService;
        _env         = env;
        _logger      = logger;
    }

    // POST /api/scenarios/activate
    // Body: { "scenarioName": "StormEvent" }
    //
    // Available scenarios:
    //   Normal         — all KRIs in green, CRS ≈ 15-25
    //   MildCongestion — berth at 75%, vessel delays at 15% (yellow)
    //   StormEvent     — weather score 67, vessel delays 32% (red)
    //   CustomsCrisis  — customs dwell time 82h (red)
    //   FullRedAlert   — all four KRIs in red band simultaneously
    //
    // This endpoint is for DEMO PURPOSES ONLY.
    // TODO: Add [Authorize(Roles = "Admin")] when auth is added.
    //       Consider removing this endpoint entirely in production.
    [HttpPost("activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ActivateScenario([FromBody] ActivateScenarioDto dto)
    {
        // Guard: only allow in Development to prevent accidental use in production
        if (!_env.IsDevelopment())
        {
            _logger.LogWarning("Attempted to activate demo scenario in non-Development environment");
            return Forbid();
        }

        var validScenarios = new[] { "Normal", "MildCongestion", "StormEvent", "CustomsCrisis", "FullRedAlert" };

        if (!validScenarios.Contains(dto.ScenarioName))
        {
            return BadRequest(new
            {
                message   = $"Unknown scenario '{dto.ScenarioName}'",
                available = validScenarios
            });
        }

        _mockService.SetScenario(dto.ScenarioName);

        _logger.LogInformation("Demo scenario activated: {scenario}", dto.ScenarioName);

        return Ok(new
        {
            message  = $"Scenario '{dto.ScenarioName}' activated",
            scenario = dto.ScenarioName
        });
    }
}
