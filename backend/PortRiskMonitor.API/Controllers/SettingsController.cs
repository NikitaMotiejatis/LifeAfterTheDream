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
    public record NotifyRequest(string KriName, double Value, string? Message);

    public record NotifyResponse(
        bool Ok,
        string Channel,
        IReadOnlyList<string> Decorators,
        IReadOnlyList<string>? Recipients,
        string? Error);

    [HttpPost("notify")]
    public async Task<IActionResult> Notify(
        [FromBody] NotifyRequest req,
        [FromServices] RiskMonitor.Services.IAlertNotifier notifier,
        [FromServices] IOptionsMonitor<NotificationOptions> opts,
        CancellationToken ct)
    {
        var current = opts.CurrentValue;
        var recipients = current.Channel switch
        {
            "Twilio" => (IReadOnlyList<string>)current.Twilio.ToNumbers,
            "Email" => current.Email.ToAddresses,
            _ => null,
        };

        try
        {
            await notifier.NotifyRedAsync(new RiskMonitor.Entities.Alert
            {
                KriName = req.KriName,
                Level = "Red",
                TriggerValue = req.Value,
                Message = req.Message ?? $"{req.KriName} entered red zone at {req.Value}",
            }, ct);

            return Ok(new NotifyResponse(true, current.Channel, current.Decorators, recipients, null));
        }
        catch (Exception ex)
        {
            return Ok(new NotifyResponse(false, current.Channel, current.Decorators, recipients, ex.Message));
        }
    }
}
