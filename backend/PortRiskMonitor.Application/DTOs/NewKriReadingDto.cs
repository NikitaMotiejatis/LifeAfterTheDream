namespace PortRiskMonitor.Application.DTOs;

public record NewKriReadingDto
{
    public double Value { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

