using RiskMonitor.DTOs;

namespace RiskMonitor.Services;

public interface IKriService
{
    Task<double?> GetLatestScore();
    IQueryable<ScoreInfo> GetScores(DateTime? from, DateTime? to);
}

