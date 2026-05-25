using Microsoft.EntityFrameworkCore;
using RiskMonitor.DTOs;
using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    Task<Kri?> GetKri();
    Task<Kri?> GetKriWithReadings(DateTime from, DateTime to);

    IQueryable<KriReading> GetAllReadings();

    Task<KriReading?> GetLatestReading()
        => GetAllReadings()
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();

    IQueryable<KriReading> GetReadings(DateTime? from, DateTime? to)
    {
        (from, to) = (from ?? DateTime.MinValue, to ?? DateTime.MaxValue);

        return GetAllReadings()
            .Where(r => from <= r.Timestamp && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp);
    }

    IQueryable<ScoreInfo> GetScores(DateTime from, DateTime to)
        => GetReadings(from, to)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            });
}
