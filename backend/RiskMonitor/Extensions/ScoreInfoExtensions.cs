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
        var buckets = await scores
            .GroupBy(s => new
            {
                TimeBucket = (s.Timestamp.Ticks - from.Ticks) / bucketLength.Ticks,
            })
            .Select(g => new ScoreInfo?[]
            {
                g.OrderBy(r => r.Value).FirstOrDefault(),
                g.OrderByDescending(r => r.Value).FirstOrDefault(),
                g.OrderBy(r => r.Timestamp).FirstOrDefault(),
                g.OrderByDescending(r => r.Timestamp).FirstOrDefault(),
            })
            .ToArrayAsync();

        return buckets
            .SelectMany(p => p)
            .Where(s => s != null)
            .Select(s => s!)
            .DistinctBy(s => s.Timestamp)
            .OrderBy(s => s.Timestamp);
    }

    public static IEnumerable<ScoreInfo> DownsampleM4Enumerable(
            this IEnumerable<ScoreInfo> scores,
            DateTime from,
            TimeSpan bucketLength)
    {
        var buckets = scores
            .GroupBy(s => new
            {
                TimeBucket = (s.Timestamp.Ticks - from.Ticks) / bucketLength.Ticks,
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

    public static async Task<IEnumerable<ScoreInfo>> DownsampleAverage(
            this IQueryable<ScoreInfo> scores,
            DateTime from,
            TimeSpan bucketLength)
    {
        var buckets = await scores
            .GroupBy(s => new
            {
                TimeBucket = (s.Timestamp.Ticks - from.Ticks) / bucketLength.Ticks,
            })
            .Select(g => new
            {
                Ticks = from.Ticks + bucketLength.Ticks * g.Key.TimeBucket,
                Value = g.Select(r => r.Value).Average(),
            })
            .OrderBy(r => r.Ticks)
            .ToListAsync();

        return buckets
            .Select(b => new ScoreInfo
            {
                Timestamp = new DateTime(b.Ticks, DateTimeKind.Utc),
                Value = b.Value,
            });
    }

    public static async Task<IEnumerable<ScoreInfo>> DownsampleAverageAsync(this IQueryable<ScoreInfo> scores, DateTime from, int bucketCount, BucketType interval)
    {
        var to = interval switch
        {
            BucketType.Day => from.AddDays(bucketCount),
            BucketType.Month => from.AddMonths(bucketCount),
            BucketType.Year => from.AddYears(bucketCount),
            BucketType.Hour or _ => from.AddDays(bucketCount),
        };

        var baseQuery = scores
            .Where(s => from <= s.Timestamp && s.Timestamp < to);

        var groupedScores = interval switch
        {
            BucketType.Day => await baseQuery
                .GroupBy(s => new
                {
                    Year = s.Timestamp.Year,
                    Month = s.Timestamp.Month,
                    Day = s.Timestamp.Day,
                })
                .ToDictionaryAsync(
                    g => from
                        .AddYears(g.Key.Year - from.Year)
                        .AddMonths(g.Key.Month - from.Month)
                        .AddDays(g.Key.Day - from.Day),
                    g => g.Average(r => r.Value)
                ),

            BucketType.Month => await baseQuery
                .GroupBy(s => new
                {
                    Year = s.Timestamp.Year,
                    Month = s.Timestamp.Month,
                })
                .ToDictionaryAsync(
                    g => from
                        .AddYears(g.Key.Year - from.Year)
                        .AddMonths(g.Key.Month - from.Month),
                    g => g.Average(r => r.Value)
                ),

            BucketType.Year => await baseQuery
                .GroupBy(s => new
                {
                    Year = s.Timestamp.Year,
                })
                .ToDictionaryAsync(
                    g => from
                        .AddYears(g.Key.Year - from.Year),
                    g => g.Average(r => r.Value)
                ),

            BucketType.Hour or _ => await baseQuery
                .GroupBy(s => new
                {
                    Year = s.Timestamp.Year,
                    Month = s.Timestamp.Month,
                    Day = s.Timestamp.Day,
                    Hour = s.Timestamp.Hour,
                })
                .ToDictionaryAsync(
                    g => from
                        .AddYears(g.Key.Year - from.Year)
                        .AddMonths(g.Key.Month - from.Month)
                        .AddDays(g.Key.Day - from.Day)
                        .AddHours(g.Key.Hour - from.Hour),
                    g => g.Average(r => r.Value)
                ),
        };

        return Enumerable
            .Range(0, bucketCount)
            .Select(bucketIdx => interval switch
            {
                BucketType.Day => from.AddDays(bucketIdx),
                BucketType.Month => from.AddMonths(bucketIdx),
                BucketType.Year => from.AddYears(bucketIdx),
                BucketType.Hour or _ => from.AddHours(bucketIdx),
            })
            .Select(bucketTime => new ScoreInfo
            {
                Timestamp = bucketTime,
                Value = groupedScores.GetValueOrDefault(bucketTime, 0.0)
            });
    }
}
