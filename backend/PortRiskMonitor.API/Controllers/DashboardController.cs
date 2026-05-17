using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Data;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IPortRiskMonitorService _portRiskMonitorService;
    //private readonly IPortStatusService _portStatusService;
    private readonly IKriRepository _repo;

    private static readonly HashSet<string> ValidSlugs =
    [
        SeedData.BerthSlug,
        SeedData.VesselDelaySlug,
        SeedData.WeatherSlug,
        SeedData.CustomsSlug,
    ];

    public DashboardController(
            IPortRiskMonitorService portRiskMonitorService,
            IKriRepository repo)
    {
        _portRiskMonitorService = portRiskMonitorService;
        //_portStatusService = portStatusService;
        _repo = repo;
    }

    //[HttpGet("/trend")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //public async Task<IActionResult> GetTrend([FromQuery] string trendTimeFrame)
    //    => Ok(_portStatusService.GetTrend(trendTimeFrame));

    [HttpGet("kri-cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKriCards([FromQuery] string preset, [FromQuery] string? from, [FromQuery] string? to)
    {
        var kriCards = await _portRiskMonitorService.GetKriCards(preset, from, to);
        return Ok(kriCards.ToArray());
    }

    // ── GET /api/history ──────────────────────────────────────────────────────
    //    [HttpGet]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    public async Task<IActionResult> GetAllLatest()
    //    {
    //        var latest = await _repo.GetLatestReadingsAsync();
    //
    //        return Ok(latest.Select(r => new
    //        {
    //            slug            = r.Kri?.Slug,
    //            kriId           = r.KriId,
    //            kriName         = r.Kri?.Name,
    //            unit            = r.Kri?.Unit,
    //            latestValue     = r.Value,
    //            timestamp       = r.Timestamp,
    //        }));
    //    }
    //
    //    // ── GET /api/history/{slug} ───────────────────────────────────────────────
    //    [HttpGet("{slug}")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    public async Task<IActionResult> GetHistory(
    //        string slug,
    //        [FromQuery] DateTime? from = null,
    //        [FromQuery] DateTime? to   = null,
    //        [FromQuery] int take       = 500)
    //    {
    //        slug = slug.ToLowerInvariant();
    //
    //        if (!ValidSlugs.Contains(slug))
    //            return BadRequest(new { error = $"Unknown slug '{slug}'.", validValues = ValidSlugs.Order() });
    //
    //        take = Math.Clamp(take, 1, 2000);
    //        var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
    //        var toDate   = to   ?? DateTime.UtcNow;
    //
    //        if (fromDate >= toDate)
    //            return BadRequest(new { error = "'from' must be earlier than 'to'." });
    //
    //        var kri = await _repo.GetBySlugAsync(slug);
    //        if (kri is null)
    //            return NotFound(new { error = $"No KRI found for slug '{slug}'. Run the app to seed data." });
    //
    //        var readings = (await _repo.GetReadingsAsync(kri.Id, fromDate, toDate, take)).ToList();
    //
    //        return Ok(new
    //        {
    //            slug,
    //            kriId      = kri.Id,
    //            kriName    = kri.Name,
    //            unit       = kri.Unit,
    //            from       = fromDate,
    //            to         = toDate,
    //            count      = readings.Count,
    //            readings   = readings.Select(r => new
    //            {
    //                timestamp       = r.Timestamp,
    //                value           = r.Value,
    //            })
    //        });
    //    }
}
