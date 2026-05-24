using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDasboardService _dashboardService;

    public DashboardController(IDasboardService dashboardService)
        => _dashboardService = dashboardService;

    [HttpGet("port-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPortStatus(
            [FromQuery] string preset,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok(await _dashboardService.GetPortStatus(preset, from, to));

    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeather()
        => Ok(await _dashboardService.GetWeather());

    [HttpGet("kri-cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetKriCards(
            [FromQuery] string preset,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok((await _dashboardService.GetKriCards(preset, from, to)).ToArray());

    [HttpGet("trend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrend([FromQuery] string trendTimeFrame)
        => Ok((await _dashboardService.GetTrend(trendTimeFrame)).ToArray());
}
