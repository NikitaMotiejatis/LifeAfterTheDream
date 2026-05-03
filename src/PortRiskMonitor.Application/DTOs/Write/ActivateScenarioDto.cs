namespace PortRiskMonitor.Application.DTOs.Write;

public record ActivateScenarioDto(
    string ScenarioName  // "Normal" | "MildCongestion" | "StormEvent" | "CustomsCrisis" | "FullRedAlert"
);