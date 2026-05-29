using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IndicatorController : ControllerBase
{
    private readonly IIndicatorService _indicatorService;

    public IndicatorController(IIndicatorService indicatorService)
        => _indicatorService = indicatorService;

    [HttpPost("readings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddReading(
            [FromQuery] string slug,
            [FromBody] NewKriReadingDto reading)
        => Ok(await _indicatorService.AddReading(slug, reading));
}
