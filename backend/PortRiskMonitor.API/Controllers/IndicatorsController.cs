using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/indicators")]
[Produces("application/json")]
public class IndicatorsController : ControllerBase
{
    private readonly IBerthOccupancyService _berth;
    private readonly IVesselDelayRateService _vessels;
    private readonly IWeatherConditionService _weather;
    private readonly ICustomsDwellTimeService _customs;

    public IndicatorsController(
        IBerthOccupancyService berth,
        IVesselDelayRateService vessels,
        IWeatherConditionService weather,
        ICustomsDwellTimeService customs)
    {
        _berth = berth;
        _vessels = vessels;
        _weather = weather;
        _customs = customs;
    }

    // GET /api/indicators/berth
    [HttpGet("berth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBerth()
    {
        var score = await _berth.GetLatestScore();

        return Ok(new
        {
            score,
            occupiedCount = _berth.GetOccupiedCount(),
            totalCount = _berth.GetTotalCount(),
            details = _berth.GetBerthDetails()
        });
    }

    // GET /api/indicators/vessel-delays
    [HttpGet("vessel-delays")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVesselDelays()
    {
        var score = await _vessels.GetLatestScore();

        return Ok(new
        {
            score,
            delayedCount = _vessels.GetDelayedCount(),
            totalCount = _vessels.GetTotalCount(),
            details = _vessels.GetDelayDetails()
        });
    }

    // GET /api/indicators/weather
    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetWeather()
        => Ok(_weather.GetWeather());

    // GET /api/indicators/customs
    [HttpGet("customs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustoms()
    {
        var score = await _customs.GetLatestScore();

        return Ok(new
        {
            score,
            avgDwellHours = _customs.GetAverageDwellHours(),
            pendingCount = _customs.GetPendingCount(),
            overdueCount = _customs.GetOverdueCount(),
            details = _customs.GetDwellDetails()
        });
    }
}
