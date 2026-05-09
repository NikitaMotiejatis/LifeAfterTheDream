namespace PortRiskMonitor.Application.DTOs.Read;

public record KriDefinitionDto(
    Guid Id,
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
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string RowVersion
);
