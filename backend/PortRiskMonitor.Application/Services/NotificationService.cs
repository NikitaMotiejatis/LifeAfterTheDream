using Microsoft.Extensions.Options;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Application.Notifications.Options;

namespace PortRiskMonitor.Application.Services;

public class NotificationService : INotificationService
{
    private readonly RiskMonitor.Services.IAlertNotifier _notifier;
    private readonly IOptionsMonitor<NotificationOptions> _opts;

    public NotificationService(RiskMonitor.Services.IAlertNotifier notifier, IOptionsMonitor<NotificationOptions> opts)
    {
        _notifier = notifier;
        _opts = opts;
    }

    public async Task<NotifyResponse> NotifyAsync(NotifyRequest req, CancellationToken ct)
    {
        var current = _opts.CurrentValue;
        var recipients = current.Channel switch
        {
            "Twilio" => (IReadOnlyList<string>)current.Twilio.ToNumbers,
            "Email" => current.Email.ToAddresses,
            _ => throw new InternalErrorException($"Unsupported notification channel: {current.Channel}"),
        };

        try
        {
            await _notifier.NotifyRedAsync(new RiskMonitor.Entities.Alert
            {
                KriName = req.KriName,
                Level = "Red",
                TriggerValue = req.Value,
                Message = req.Message ?? $"{req.KriName} entered red zone at {req.Value}",
            }, ct);

            return new NotifyResponse(true, current.Channel, current.Decorators, recipients, null);
        }
        catch (Exception ex)
        {
            return new NotifyResponse(false, current.Channel, current.Decorators, recipients, ex.Message);
        }
    }
}
