namespace PortRiskMonitor.Application.DTOs.Write;

public record CreateKriDto(
    string Name,
    string Unit,
    string Description,
    string FormulaLabel,
    double GreenMax,
    double YellowMax,
    double Weight,
    bool   HigherIsWorse,
    double MockBaseline,
    double MockVariance,
    string MockPattern
);