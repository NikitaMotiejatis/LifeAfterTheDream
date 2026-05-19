using RiskMonitor.DTOs;
using RiskMonitor.Repositories;

namespace RiskMonitor.Services;

public abstract class KriService : IKriService
{
    protected readonly IKriRepository _kriRepo;

    protected KriService(IKriRepository kriRepo)
    {
        _kriRepo = kriRepo;
    }

    public async Task<double?> GetLatestScore()
    {
        var latestReading = await _kriRepo.GetLatestReading();
        return latestReading?.Value;
    }

    public IQueryable<ScoreInfo> GetScores(DateTime? from, DateTime? to)
    {
        (from, to) = (from ?? DateTime.MinValue, to ?? DateTime.MaxValue);
        return _kriRepo
            .GetAllReadings()
                .Where(r => from <= r.Timestamp && r.Timestamp <= to)
                .OrderBy(r => r.Timestamp)
                .Select(r => new ScoreInfo
                {
                    Timestamp = r.Timestamp,
                    Value = r.Value,
                });
    }
}
