namespace PortRiskMonitor.Infrastructure.Notifications.Options;

public class NotificationOptions
{
    public const string SectionName = "Notifications";

    public string Channel { get; set; } = "Off";
    public List<string> Decorators { get; set; } = new();
    public TwilioOptions Twilio { get; set; } = new();
    public EmailOptions Email { get; set; } = new();
}
