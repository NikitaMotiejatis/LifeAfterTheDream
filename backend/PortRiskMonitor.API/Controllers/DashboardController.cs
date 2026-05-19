using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IPortRiskMonitorService _portRiskMonitorService;
    private readonly IPortStatusService _portStatusService;
    private readonly IWeatherFetcherService _weatherFetcer;

    public DashboardController(
            IPortRiskMonitorService portRiskMonitorService,
            IPortStatusService portStatusService,
            IWeatherFetcherService weatherFetcer)

    {
        _portRiskMonitorService = portRiskMonitorService;
        _portStatusService = portStatusService;
        _weatherFetcer = weatherFetcer;
    }

    [HttpGet("port-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortStatus([FromQuery] string preset, [FromQuery] string? from, [FromQuery] string? to)
        => Ok(await _portStatusService.GetPortStatus(preset, from, to));

    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeather()
    {
        var weatherSnapshot = await _weatherFetcer.GetLatestWeatherSnapshot();
        return Ok(new
        {
            windSpeedKts = weatherSnapshot.WindSpeedKnt,
            waveHeightM = 0.01 * weatherSnapshot.WaterLevelCm,
            temperatureC = weatherSnapshot.TemperatureC,
            humidityPercent = weatherSnapshot.HumidityPercent,
            description = weatherSnapshot.ConditionCode,
        });
    }

    [HttpGet("kri-cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKriCards([FromQuery] string preset, [FromQuery] string? from, [FromQuery] string? to)
        => Ok((await _portRiskMonitorService.GetKriCards(preset, from, to)).ToArray());

    [HttpGet("trend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrend([FromQuery] string trendTimeFrame)
        => Ok((await _portStatusService.GetTrend(trendTimeFrame)).ToArray());

}
