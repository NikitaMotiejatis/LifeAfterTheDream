namespace PortRiskMonitor.Application.DTOs.Write;

public record OverrideKriValueDto(
    double Value    // the value to force — will be evaluated against thresholds immediately
);