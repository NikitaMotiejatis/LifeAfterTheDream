using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IPortRiskMonitorService _portRiskMonitorService;
    private readonly IPortStatusService _portStatusService;
    private readonly IWeatherConditionService _weatherConditionService;

    public DashboardController(
            IPortRiskMonitorService portRiskMonitorService,
            IPortStatusService portStatusService,
            IWeatherConditionService weatherFetcer)

    {
        _portRiskMonitorService = portRiskMonitorService;
        _portStatusService = portStatusService;
        _weatherConditionService = weatherFetcer;
    }

    [HttpGet("port-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPortStatus(
            [FromQuery] string preset,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok(await _portStatusService.GetPortStatus(preset, from, to));

    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetWeather()
        => Ok(_weatherConditionService.GetWeather());

    [HttpGet("kri-cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetKriCards(
            [FromQuery] string preset,
            [FromQuery] string? from,
            [FromQuery] string? to)
        => Ok((await _portRiskMonitorService.GetKriCards(preset, from, to)).ToArray());

    [HttpGet("trend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTrend([FromQuery] string trendTimeFrame)
        => Ok((await _portStatusService.GetTrend(trendTimeFrame)).ToArray());

}
