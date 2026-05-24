namespace PortRiskMonitor.Infrastructure.Notifications;

public class SmsAlertOptions
{
    public const string SectionName = "Alerts:Sms";

    public bool Enabled { get; set; } = false;
    public string AwsRegion { get; set; } = "us-east-1";
    public List<string> RecipientPhoneNumbers { get; set; } = new();
}
