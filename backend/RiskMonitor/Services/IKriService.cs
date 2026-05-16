namespace RiskMonitor.Services;

public interface IKriService
{
    Task<double> GetLatestScore();
    Task<IEnumerable<(DateTime Timestamp, double Value)>> GetScores(DateTime? from, DateTime? to);
}

