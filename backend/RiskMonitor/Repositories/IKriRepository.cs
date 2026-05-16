using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    Task<Kri> GetKri();
    Task<IEnumerable<KriReading>> GetAllReadings();

    Task<KriReading?> GetLatestReading()
        => GetAllReadings()
            .ContinueWith(task => task.Result
                .OrderBy(r => r.Timestamp)
                .LastOrDefault()
            );

    Task<IEnumerable<KriReading>> GetReadings(DateTime? from, DateTime? to)
        => GetAllReadings()
            .ContinueWith(task => task.Result
                .Where(r =>
                    (from ?? DateTime.MinValue) <= r.Timestamp
                    && r.Timestamp <= (to ?? DateTime.MaxValue)
                ).OrderBy(r => r.Timestamp)
                .AsEnumerable()
            );
}
