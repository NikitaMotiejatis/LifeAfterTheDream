using RiskMonitor.DTOs;

namespace RiskMonitor.Logic;

public interface IRiskCalculator<T>
{
    public RiskLevel CalculateRisk(T score);
}
