using Microsoft.AspNetCore.Mvc;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;

namespace PortRiskMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpPost("notify")]
    public async Task<IActionResult> Notify([FromBody] NotifyRequest req, CancellationToken ct)
        => Ok(await _service.NotifyAsync(req, ct));
}
