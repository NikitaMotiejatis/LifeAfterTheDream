using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.Notifications.Options;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly IThresholdSettingsService _service;

    public SettingsController(IThresholdSettingsService service)
    {
        _service = service;
    }

    [HttpGet("thresholds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThresholds()
        => Ok(await _service.GetAllAsync());

    [HttpPut("thresholds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateThresholds([FromBody] ThresholdSettingsDto settings)
        => Ok(await _service.UpdateAsync(settings));

    [HttpPut("thresholds/{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateThreshold(string slug, [FromBody] ThresholdPairDto pair)
        => Ok(await _service.UpdateOneAsync(slug, pair));


    [HttpPost("thresholds/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetThresholds()
        => Ok(await _service.ResetAsync());
}
