using Microsoft.EntityFrameworkCore;

using RiskMonitor.Entities;

namespace RiskMonitor.Repositories;

public interface IKriRepository
{
    Task<Kri> GetKri();
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
}