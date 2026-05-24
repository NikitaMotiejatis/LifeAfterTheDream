using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;

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
    public async Task<IActionResult> UpdateThresholds([FromBody] ThresholdSettingsDto settings)
        => Ok(await _service.UpdateAsync(settings));

    [HttpPost("thresholds/reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetThresholds()
        => Ok(await _service.ResetAsync());
}
