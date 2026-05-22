using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IPortRiskMonitorService _portRiskMonitorService;
    private readonly IWeatherFetcherService _weatherFetcer;

    public AnalyticsController(
        IPortRiskMonitorService portRiskMonitorService,
        IWeatherFetcherService weatherFetcer)
    {
        _portRiskMonitorService = portRiskMonitorService;
        _weatherFetcer = weatherFetcer;
    }

    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics(
            [FromRoute] string slug,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok(await _portRiskMonitorService.GetAnalytics(slug, from, to));
}
