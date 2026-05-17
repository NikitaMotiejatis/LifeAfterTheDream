// ============================================================
// HistoryController.cs — Historical KRI data
//
// Routes:
//   GET /api/history                  — latest reading for every indicator
//   GET /api/history/{slug}           — time-series for one indicator
//
// Query params for the detail route:
//   from  (DateTime UTC)  default: 30 days ago
//   to    (DateTime UTC)  default: now
//   take  (int)           default: 500, max: 2000
//
// Valid slugs: berth | vessel-delays | weather | customs
// ============================================================

using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/history")]
[Produces("application/json")]
public class HistoryController : ControllerBase
{
    private readonly IKriRepository _repo;

    private static readonly HashSet<string> ValidSlugs =
    [
        SeedData.BerthSlug,
        SeedData.VesselDelaySlug,
        SeedData.WeatherSlug,
        SeedData.CustomsSlug,
    ];

    public HistoryController(IKriRepository repo) => _repo = repo;

    // ── GET /api/history ──────────────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllLatest()
    {
        var latest = await _repo.GetLatestReadingsAsync();

        return Ok(latest.Select(r => new
        {
            slug            = r.Kri?.Slug,
            kriId           = r.KriId,
            kriName         = r.Kri?.Name,
            unit            = r.Kri?.Unit,
            latestValue     = r.Value,
            riskLevel       = r.RiskLevel,
            timestamp       = r.Timestamp,
        }));
    }

    // ── GET /api/history/{slug} ───────────────────────────────────────────────
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        string slug,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null,
        [FromQuery] int take       = 500)
    {
        slug = slug.ToLowerInvariant();

        if (!ValidSlugs.Contains(slug))
            return BadRequest(new { error = $"Unknown slug '{slug}'.", validValues = ValidSlugs.Order() });

        take = Math.Clamp(take, 1, 2000);
        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
        var toDate   = to   ?? DateTime.UtcNow;

        if (fromDate >= toDate)
            return BadRequest(new { error = "'from' must be earlier than 'to'." });

        var kri = await _repo.GetBySlugAsync(slug);
        if (kri is null)
            return NotFound(new { error = $"No KRI found for slug '{slug}'. Run the app to seed data." });

        var readings = (await _repo.GetReadingsAsync(kri.Id, fromDate, toDate, take)).ToList();

        return Ok(new
        {
            slug,
            kriId      = kri.Id,
            kriName    = kri.Name,
            unit       = kri.Unit,
            thresholds = new { greenMax = kri.GreenMax, yellowMax = kri.YellowMax },
            from       = fromDate,
            to         = toDate,
            count      = readings.Count,
            readings   = readings.Select(r => new
            {
                timestamp       = r.Timestamp,
                value           = r.Value,
                riskLevel       = r.RiskLevel,
                isSimulated     = r.IsSimulated,
            })
        });
    }

    [HttpGet("cards")]
    public async Task<IActionResult> GetKriCards([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int take = 20)
    {
        var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
        var toDate = to ?? DateTime.UtcNow;

        var kris = (await _repo.GetAllAsync()).OrderBy(k => k.CreatedAt).ToList();
        var cards = new List<object>();

        foreach (var kri in kris)
        {
            var readings = (await _repo.GetReadingsAsync(kri.Id, fromDate, toDate, 2000)).ToList();
            var latest = readings.LastOrDefault();

            // Sample down to ~20 points for the sparkline
            var sparkline = readings.Count <= take ? readings
                : Enumerable.Range(0, take)
                    .Select(i => readings[(int)Math.Round((double)i / (take-1) * (readings.Count - 1))]);

            cards.Add(new
            {
                id = kri.Slug,
                title = kri.Name,
                value = latest is null ? "–" : kri.Unit == "%" ? $"{latest.Value:F1}%" : kri.Unit == "hours" ? $"{latest.Value:F1}h" : $"{latest.Value:F1}",
                unit = kri.Unit,
                riskLevel = latest?.RiskLevel ?? "Green",
                thresholds = new[] {
                new { label = $"<{kri.GreenMax}",                  color = "#22c55e", severity = "Low"    },
                new { label = $"{kri.GreenMax}–{kri.YellowMax}",   color = "#eab308", severity = "Medium" },
                new { label = $">{kri.YellowMax}",                 color = "#ef4444", severity = "High"   },
            },
                sparkline = sparkline.Select(r => new { timestamp = r.Timestamp, value = r.Value }),
            });
        }

        return Ok(cards);
    }
}
