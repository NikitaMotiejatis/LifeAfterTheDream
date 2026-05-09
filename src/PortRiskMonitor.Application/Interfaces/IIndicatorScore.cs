// ============================================================
// IIndicatorScore.cs — Generic base interface for all KRI services
// ============================================================

namespace PortRiskMonitor.Application.Interfaces;

using System;
using System.Collections.Generic;

public interface IIndicatorScore
{
    double GetScoreValue();
    string FormatScore(double score);
    ICollection<(DateTime Timestamp, double Score)> GetScores(DateTime? from = null, DateTime? to = null);
}
