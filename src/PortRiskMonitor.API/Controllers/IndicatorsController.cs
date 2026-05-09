using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.DTOs.Enums;
using PortRiskMonitor.Application.DTOs.Read;
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
    public IActionResult GetBerth() => Ok(new
    {
        score = _berth.GetScoreValue(),
        occupiedCount = _berth.GetOccupiedCount(),
        totalCount = _berth.GetTotalCount(),
        details = _berth.GetBerthDetails()
    });

    // GET /api/indicators/vessel-delays
    [HttpGet("vessel-delays")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetVesselDelays() => Ok(new
    {
        score = _vessels.GetScoreValue(),
        delayedCount = _vessels.GetDelayedCount(),
        totalCount = _vessels.GetTotalCount(),
        details = _vessels.GetDelayDetails()
    });

    // GET /api/indicators/weather
    [HttpGet("weather")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetWeather() => Ok(new
    {
        score = _weather.GetScoreValue(),
        windSpeedKnt = _weather.GetWindSpeedKnt(),
        waterLevelCm = _weather.GetWaterLevelCm(),
        temperatureC = _weather.GetTemperatureC(),
        humidityPercent = _weather.GetHumidityPercent(),
        conditionCode = _weather.GetConditionCode()
    });

    // GET /api/indicators/customs
    [HttpGet("customs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCustoms() => Ok(new
    {
        score = _customs.GetScoreValue(),
        avgDwellHours = _customs.GetAverageDwellHours(),
        pendingCount = _customs.GetPendingCount(),
        overdueCount = _customs.GetOverdueCount(),
        details = _customs.GetDwellDetails()
    });
}
