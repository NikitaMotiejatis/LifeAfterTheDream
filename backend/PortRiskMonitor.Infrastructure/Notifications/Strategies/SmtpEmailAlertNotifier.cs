using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PortRiskMonitor.Infrastructure.Notifications.Options;
using RiskMonitor.Entities;
using RiskMonitor.Services;

namespace PortRiskMonitor.Infrastructure.Notifications.Strategies;

public class SmtpEmailAlertNotifier : IAlertNotifier
{
    private readonly IOptionsMonitor<NotificationOptions> _options;
    private readonly ILogger<SmtpEmailAlertNotifier> _logger;

    public SmtpEmailAlertNotifier(
        IOptionsMonitor<NotificationOptions> options,
        ILogger<SmtpEmailAlertNotifier> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task NotifyRedAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var email = _options.CurrentValue.Email;

        if (string.IsNullOrWhiteSpace(email.SmtpHost) || string.IsNullOrWhiteSpace(email.FromAddress))
        {
            _logger.LogWarning("SmtpEmailAlertNotifier: missing SmtpHost/FromAddress — skipping email");
            return;
        }
        if (email.ToAddresses.Count == 0)
        {
            _logger.LogWarning("SmtpEmailAlertNotifier: no recipients configured — skipping email");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(email.FromAddress));
        foreach (var to in email.ToAddresses)
            message.To.Add(MailboxAddress.Parse(to));

        message.Subject = $"[Port Risk] RED: {alert.KriName} = {alert.TriggerValue}";
        message.Body = new TextPart("plain")
        {
            Text = $"RED alert\n\nKRI: {alert.KriName}\nValue: {alert.TriggerValue}\nMessage: {alert.Message}\nTime: {alert.TriggeredAt:u}\n",
        };

        using var client = new SmtpClient();
        try
        {
            var secureOption = email.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(email.SmtpHost, email.SmtpPort, secureOption, cancellationToken);

            if (!string.IsNullOrWhiteSpace(email.Username) && !string.IsNullOrWhiteSpace(email.AppPassword))
                await client.AuthenticateAsync(email.Username, email.AppPassword, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "SMTP email sent to {Count} recipient(s) for {KriName}",
                email.ToAddresses.Count, alert.KriName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP email send failed (host {Host})", email.SmtpHost);
            throw;
        }
    }
}
