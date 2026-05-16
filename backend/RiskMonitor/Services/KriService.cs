using RiskMonitor.Repositories;

namespace RiskMonitor.Services;

public abstract class KriService : IKriService
{
    protected readonly IKriRepository _kriRepo;

    protected KriService(IKriRepository kriRepo)
    {
        _kriRepo = kriRepo;
    }

    public Task<double> GetLatestScore()
        => _kriRepo
            .GetAllReadings()
            .ContinueWith(task => task.Result
                .OrderBy(r => r.Timestamp)
                .Select(r => r.Value)
                .LastOrDefault(0.0));

    public Task<IEnumerable<(DateTime Timestamp, double Value)>> GetScores(DateTime? from, DateTime? to)
        => _kriRepo
            .GetAllReadings()
            .ContinueWith(task => task.Result
                .Where(r =>
                    (from ?? DateTime.MinValue) <= r.Timestamp
                    && r.Timestamp <= (to ?? DateTime.MaxValue)
                ).OrderBy(r => r.Timestamp)
                .Select(r => (Timestamp: r.Timestamp, Value: r.Value))
            );
}
