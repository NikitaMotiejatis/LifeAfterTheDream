// ============================================================
// HistoryController.cs — Historical KRI data endpoint
//
// Routes:
//   GET /api/history                — latest reading for every indicator (dashboard overview)
//   GET /api/history/{indicator}    — time-series readings for one indicator
//
// Query parameters for the detail route:
//   from  (DateTime, UTC) — start of range. Default: 30 days ago.
//   to    (DateTime, UTC) — end of range.   Default: now.
//   take  (int)           — max data points returned. Default 500, capped at 2000.
//
// Valid indicator slugs: berth | vessel-delays | weather | customs
// (These match the FormulaLabel values inserted by SeedData.cs)
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Data;
using PortRiskMonitor.Infrastructure.Repositories;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/history")]
[Produces("application/json")]
public class HistoryController : ControllerBase
{
    private readonly IKriRepository _kriRepo;
    private readonly AppDbContext _db;

    private static readonly HashSet<string> ValidSlugs =
        [SeedData.BerthSlug, SeedData.VesselDelaySlug, SeedData.WeatherSlug, SeedData.CustomsSlug];

    public HistoryController(IKriRepository kriRepo, AppDbContext db)
    {
        _kriRepo = kriRepo;
        _db = db;
    }

    // ── GET /api/history ─────────────────────────────────────────────────────
    /// <summary>
    /// Returns the most recent reading for each indicator.
    /// Useful for the dashboard to confirm data exists and see current risk level.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLatest()
    {
        var latest = await _kriRepo.GetLatestReadingsAsync();

        return Ok(latest.Select(r => new
        {
            indicator     = r.KriDefinition?.FormulaLabel,
            kriId         = r.KriDefinitionId,
            kriName       = r.KriDefinition?.Name,
            unit          = r.KriDefinition?.Unit,
            latestValue   = r.Value,
            latestScore   = r.NormalizedScore,
            riskLevel     = r.RiskLevel,
            timestamp     = r.Timestamp,
        }));
    }

    // ── GET /api/history/{indicator} ─────────────────────────────────────────
    /// <summary>
    /// Returns a time-series of readings for a single indicator.
    /// </summary>
    /// <param name="indicator">berth | vessel-delays | weather | customs</param>
    /// <param name="from">UTC start. Defaults to 30 days ago.</param>
    /// <param name="to">UTC end. Defaults to now.</param>
    /// <param name="take">Max points returned (1–2000). Default 500.</param>
    [HttpGet("{indicator}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        string indicator,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null,
        [FromQuery] int take       = 500)
    {
        indicator = indicator.ToLowerInvariant();

        if (!ValidSlugs.Contains(indicator))
            return BadRequest(new
            {
                error = $"Unknown indicator '{indicator}'.",
                validValues = ValidSlugs.Order()
            });

        take = Math.Clamp(take, 1, 2000);
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate   = to   ?? DateTime.UtcNow;

        if (fromDate >= toDate)
            return BadRequest(new { error = "'from' must be earlier than 'to'." });

        // Resolve slug → KriDefinition (stored in FormulaLabel by SeedData)
        var kri = await _db.KriDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.FormulaLabel == indicator);

        if (kri is null)
            return NotFound(new
            {
                error   = $"No KRI definition found for '{indicator}'.",
                hint    = "Make sure SeedData.SeedAsync() ran on startup."
            });

        var readings = await _kriRepo.GetReadingsAsync(kri.Id, fromDate, toDate, take);
        var list     = readings.ToList(); // materialise once

        return Ok(new
        {
            indicator,
            kriId      = kri.Id,
            kriName    = kri.Name,
            unit       = kri.Unit,
            thresholds = new { greenMax = kri.GreenMax, yellowMax = kri.YellowMax },
            from       = fromDate,
            to         = toDate,
            count      = list.Count,
            readings   = list.Select(r => new
            {
                timestamp       = r.Timestamp,
                value           = r.Value,
                normalizedScore = r.NormalizedScore,
                riskLevel       = r.RiskLevel,
                isSimulated     = r.IsSimulated,
            })
        });
    }
}
