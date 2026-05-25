using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
        => _analyticsService = analyticsService;

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(
            [FromRoute] string slug,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok(await _analyticsService.GetAnalytics(slug, from, to));
}
