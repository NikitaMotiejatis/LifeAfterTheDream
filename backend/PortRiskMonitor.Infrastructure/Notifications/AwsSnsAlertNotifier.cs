using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiskMonitor.Entities;
using RiskMonitor.Services;

namespace PortRiskMonitor.Infrastructure.Notifications;

// Publishes a one-shot SMS via AWS SNS to each recipient configured in
// Alerts:Sms:RecipientPhoneNumbers. AWS credentials come from the standard SDK
// chain (env vars, user secrets, IAM role) — never committed.
public class AwsSnsAlertNotifier : IAlertNotifier
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly SmsAlertOptions _options;
    private readonly ILogger<AwsSnsAlertNotifier> _logger;

    public AwsSnsAlertNotifier(
        IAmazonSimpleNotificationService sns,
        IOptions<SmsAlertOptions> options,
        ILogger<AwsSnsAlertNotifier> logger)
    {
        _sns = sns;
        _options = options.Value;
        _logger = logger;
    }

    public async Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        if (_options.RecipientPhoneNumbers.Count == 0)
        {
            _logger.LogWarning("AwsSnsAlertNotifier: no recipients configured — skipping SMS");
            return;
        }

        var body = $"[Port Risk] RED: {alert.KriName} = {alert.TriggerValue} ({alert.Message})";

        foreach (var number in _options.RecipientPhoneNumbers)
        {
            try
            {
                await _sns.PublishAsync(new PublishRequest
                {
                    PhoneNumber = number,
                    Message = body,
                }, cancellationToken);

                _logger.LogInformation("SNS SMS sent to {Number} for {KriName}", number, alert.KriName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SNS SMS to {Number}", number);
                // Continue to the next recipient — one bad number doesn't block the rest.
            }
        }
    }
}
