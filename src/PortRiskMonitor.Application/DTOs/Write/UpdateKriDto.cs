namespace PortRiskMonitor.Application.DTOs.Write;

public record UpdateKriDto(
    string Name,
    string Unit,
    string Description,
    string FormulaLabel,
    double GreenMax,
    double YellowMax,
    double Weight,
    bool HigherIsWorse,
    double MockBaseline,
    double MockVariance,
    string MockPattern,
    string RowVersion   // REQUIRED — must match current DB value or 409 is returned
);
