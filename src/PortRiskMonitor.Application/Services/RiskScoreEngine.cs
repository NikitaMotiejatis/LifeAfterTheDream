// ============================================================
// RiskScoreEngine.cs — Business logic for risk level evaluation
//
// This is the mathematical core of the system. It contains
// all formulas for converting raw KRI values into normalized
// scores and composite risk levels.
//
// It is STATELESS — it takes inputs and returns outputs.
// No DB calls, no side effects. Easy to unit test.
// ============================================================

using PortRiskMonitor.Application.DTOs.Read;
using PortRiskMonitor.Application.DTOs.Shared;
using PortRiskMonitor.Application.Interfaces;

using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.Services;

public class RiskScoreEngine : IRiskScoreEngine
{
    private readonly IRiskScoreStrategy _strategy;

    // Strategy is injected — which strategy is used depends on appsettings.json
    // NFR: Extensibility — swap algorithm via config, no recompile needed
    public RiskScoreEngine(IRiskScoreStrategy strategy)
    {
        _strategy = strategy;
    }

    public RiskLevel EvaluateRiskLevel(
        double value,
        double greenMax,
        double yellowMax,
        bool higherIsWorse)
    {
        if (higherIsWorse)
        {
            if (value <= greenMax) return RiskLevel.Low;
            if (value <= yellowMax) return RiskLevel.Medium;
            return RiskLevel.High;
        }
        else
        {
            if (value >= greenMax) return RiskLevel.Low;
            if (value >= yellowMax) return RiskLevel.Medium;
            return RiskLevel.High;
        }
    }

    public double NormalizeScore(
        double value,
        double greenMax,
        double yellowMax,
        bool higherIsWorse)
    {
        // Invert value for lower-is-worse metrics
        if (!higherIsWorse)
        {
            value = greenMax + yellowMax - value; // Simple inversion
        }

        if (value <= greenMax)
        {
            return (value / greenMax) * 33;
        }
        else if (value <= yellowMax)
        {
            var posInYellow = (value - greenMax) / (yellowMax - greenMax);
            return 33 + posInYellow * 33;
        }
        else
        {
            var posInRed = (value - yellowMax) / yellowMax;
            return Math.Min(100, 66 + posInRed * 34);
        }
    }

    public CompositeScoreDto CalculateCompositeScore(IEnumerable<KriScoreInputDto> inputs)
    {
        // Delegate the actual calculation to the injected strategy
        // This method just interprets the result and adds the description label
        var score = _strategy.Calculate(inputs);

        var level = score <= 33 ? RiskLevel.Low
                  : score <= 66 ? RiskLevel.Medium
                  : RiskLevel.High;

        var description = level switch
        {
            RiskLevel.Low => "Normal Operations",
            RiskLevel.Medium => "Monitor Closely",
            RiskLevel.High => "Action Required",
            _ => "Unknown"
        };

        return new CompositeScoreDto(score, level, description);
    }
}

// ────────────────────────────────────────────────────────────
// STRATEGY IMPLEMENTATIONS
// NFR: Extensibility — each is a separate class, old ones never modified
// Activated by changing "RiskScoring:Strategy" in appsettings.json
// ────────────────────────────────────────────────────────────

// Strategy 1: Weighted sum (default)
// CRS = Σ(normalizedScore_i × weight_i)
// This is the standard approach — higher-risk KRIs with higher weights
// pull the composite score up more than lower-weight KRIs.
public class DefaultWeightedStrategy : IRiskScoreStrategy
{
    public double Calculate(IEnumerable<KriScoreInputDto> inputs)
    {
        return inputs.Sum(i => i.NormalizedScore * i.Weight);
    }
}

// Strategy 2: Max risk (takes the worst single KRI)
// CRS = MAX(normalizedScore_i)
// Use this when any single red KRI should immediately elevate the composite to red.
// More conservative than the weighted approach.
public class MaxRiskStrategy : IRiskScoreStrategy
{
    public double Calculate(IEnumerable<KriScoreInputDto> inputs)
    {
        return inputs.Max(i => i.NormalizedScore);
    }
}

// Strategy 3: Simple average
// CRS = Σ(normalizedScore_i) / count
// Use this when all KRIs are equally important and weights should be ignored.
public class AverageRiskStrategy : IRiskScoreStrategy
{
    public double Calculate(IEnumerable<KriScoreInputDto> inputs)
    {
        return inputs.Average(i => i.NormalizedScore);
    }
}
