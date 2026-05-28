using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortRiskMonitor.Application.Notifications.Options;
using RiskMonitor.Entities;
using RiskMonitor.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace PortRiskMonitor.Application.Notifications.Strategies;

public class TwilioSmsAlertNotifier : IAlertNotifier
{
    private readonly IOptionsMonitor<NotificationOptions> _options;
    private readonly ILogger<TwilioSmsAlertNotifier> _logger;

    public TwilioSmsAlertNotifier(
        IOptionsMonitor<NotificationOptions> options,
        ILogger<TwilioSmsAlertNotifier> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var twilio = _options.CurrentValue.Twilio;

        if (string.IsNullOrWhiteSpace(twilio.AccountSid) || string.IsNullOrWhiteSpace(twilio.AuthToken))
        {
            _logger.LogWarning("TwilioSmsAlertNotifier: missing AccountSid/AuthToken — skipping SMS");
            return;
        }
        if (string.IsNullOrWhiteSpace(twilio.FromNumber))
        {
            _logger.LogWarning("TwilioSmsAlertNotifier: missing FromNumber — skipping SMS");
            return;
        }
        if (twilio.ToNumbers.Count == 0)
        {
            _logger.LogWarning("TwilioSmsAlertNotifier: no recipients configured — skipping SMS");
            return;
        }

        TwilioClient.Init(twilio.AccountSid, twilio.AuthToken);
        var body = $"[Port Risk] RED: {alert.KriName} = {alert.TriggerValue} ({alert.Message})";

        foreach (var to in twilio.ToNumbers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var message = await MessageResource.CreateAsync(
                    to: new PhoneNumber(to),
                    from: new PhoneNumber(twilio.FromNumber),
                    body: body);

                _logger.LogInformation(
                    "Twilio SMS sent to {To} (SID: {Sid}) for {KriName}",
                    to, message.Sid, alert.KriName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Twilio send failed to {To}", to);
                throw;
            }
        }
    }
}
