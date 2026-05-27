namespace PortRiskMonitor.Infrastructure.Notifications.Options;

public class TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
    public List<string> ToNumbers { get; set; } = new();
}
