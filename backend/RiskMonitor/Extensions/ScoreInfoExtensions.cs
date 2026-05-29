using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

using RiskMonitor.DTOs;

namespace RiskMonitor.Extensions;

public static class ScoreInfoExtentions
{
    public static async Task<IEnumerable<ScoreInfo>> DownsampleM4Async(
            this IQueryable<ScoreInfo> scores,
            DateTime from,
            TimeSpan bucketLength)
    {
        var bucketSeconds = bucketLength.TotalSeconds;
        var buckets = await scores
            .GroupBy(s => new
            {
                TimeBucket = Math.Floor((s.Timestamp - from).TotalSeconds / bucketSeconds),
            })
            .Select(g => new ScoreInfo?[]
            {
                g.OrderBy(r => r.Value).FirstOrDefault(),
                g.OrderByDescending(r => r.Value).FirstOrDefault(),
                g.OrderBy(r => r.Timestamp).FirstOrDefault(),
                g.OrderByDescending(r => r.Timestamp).FirstOrDefault(),
            })
            .ToListAsync();

        return buckets
            .SelectMany(p => p)
            .Where(s => s != null)
            .Select(s => s!)
            .DistinctBy(s => s.Timestamp)
            .OrderBy(s => s.Timestamp);
    }

    public static IEnumerable<ScoreInfo> DownsampleM4(
            this IEnumerable<ScoreInfo> scores,
            DateTime from,
            TimeSpan bucketLength)
    {
        var bucketSeconds = bucketLength.TotalSeconds;
        var buckets = scores
            .GroupBy(s => new
            {
                TimeBucket = Math.Floor((s.Timestamp - from).TotalSeconds / bucketSeconds),
            })
            .Select(g => new ScoreInfo?[]
            {
                g.OrderBy(r => r.Value).FirstOrDefault(),
                g.OrderByDescending(r => r.Value).FirstOrDefault(),
                g.OrderBy(r => r.Timestamp).FirstOrDefault(),
                g.OrderByDescending(r => r.Timestamp).FirstOrDefault(),
            });

        return buckets
            .SelectMany(p => p)
            .Where(s => s != null)
            .Select(s => s!)
            .DistinctBy(s => s.Timestamp)
            .OrderBy(s => s.Timestamp);
    }

    public static async Task<IEnumerable<ScoreInfo>> DownsampleAverageAsync(
            this IQueryable<ScoreInfo> scores,
            DateTime from,
            int bucketCount,
            BucketType interval)
    {
        Expression<Func<ScoreInfo, DateTime>> grouping = interval switch
        {
            BucketType.Hour => (s) => new DateTime(s.Timestamp.Year, s.Timestamp.Month, s.Timestamp.Day, s.Timestamp.Hour, 0, 0, DateTimeKind.Utc),
            BucketType.Day => (s) => new DateTime(s.Timestamp.Year, s.Timestamp.Month, s.Timestamp.Day, 0, 0, 0, DateTimeKind.Utc),
            BucketType.Month => (s) => new DateTime(s.Timestamp.Year, s.Timestamp.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            BucketType.Year => (s) => new DateTime(s.Timestamp.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => throw new InvalidEnumArgumentException($"BucketType value {interval} is not handled"),
        };

        var groupedScores = await scores
            .GroupBy(grouping)
            .Select(g => new
            {
                BucketTime = g.Key,
                AverageValue = g.Average(r => r.Value)
            })
            .ToDictionaryAsync(
                x => DateTime.SpecifyKind(x.BucketTime, DateTimeKind.Utc),
                x => x.AverageValue
            );

        return Enumerable
            .Range(0, bucketCount)
            .Select(bucketIndex => interval switch
            {
                BucketType.Hour => from.AddHours(bucketIndex),
                BucketType.Day => from.AddDays(bucketIndex),
                BucketType.Month => from.AddMonths(bucketIndex),
                BucketType.Year => from.AddYears(bucketIndex),
                _ => throw new InvalidEnumArgumentException($"BucketType value {interval} is not handled"),
            })
            .Select(bucketTime => new ScoreInfo
            {
                Timestamp = bucketTime,
                Value = groupedScores.GetValueOrDefault(bucketTime, 0.0)
            });
    }
}
