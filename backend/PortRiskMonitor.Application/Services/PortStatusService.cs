using System.Globalization;
using Microsoft.EntityFrameworkCore;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.PortStatus;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Services;

public class PortStatusService : KriService, IPortStatusService
{
    private IPortStatusRepo _portStatusRepo;

    public PortStatusService(IPortStatusRepo portStatusRepo)
        : base(portStatusRepo)
    {
        _portStatusRepo = portStatusRepo;
    }

    public async Task<PortStatusDto> GetPortStatus(string preset, string? from, string? to)
    {
        var now = DateTime.UtcNow;
        var format = "yyyy-MM-ddTHH:mm";
        var provider = CultureInfo.InvariantCulture;

        var fromDateTime = preset switch
        {
            "6h" => now.AddHours(-6),
            "12h" => now.AddHours(-12),
            "24h" => now.AddHours(-24),
            "48h" => now.AddHours(-48),
            "72h" => now.AddHours(-72),
            "week" => now.AddDays(-7),
            "month" => now.AddMonths(-1),
            "year" => now.AddYears(-1),
            _ => now.AddHours(-24),
        };
        var toDateTime = now;

        try
        {
            fromDateTime = DateTime.ParseExact(from ?? "", format, provider);
            toDateTime = DateTime.ParseExact(to ?? "", format, provider);
        }
#pragma warning disable CS0168
        catch (Exception e) { }
#pragma warning restore CS0168

        var disruptionIndex = await GetLatestScore() ?? 0.0;

        var rawScores = await GetScores(fromDateTime, toDateTime).ToListAsync();

        var sparkline = rawScores
            .Select(s => new PortStatusDto.SparkPoint
            {
                Label = s.Timestamp.ToLocalTime().ToString(format, provider),
                Value = s.Value,
            }).ToList();

        return new PortStatusDto
        {
            DisruptionIndex = disruptionIndex,
            RiskLevel = "Moderate",
            Sparkline = sparkline,
        };
    }

    public async Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame)
    {
        var now = DateTime.UtcNow;

        var bucketCount = trendTimeFrame switch
        {
            "7d" => 7,
            "30d" => 30,
            "90d" => 90,
            "6m" => 6,
            "1y" => 12,
            "24h" or _ => 24,
        };

        var from = trendTimeFrame switch
        {
            "7d" or "30d" or "90d"
                => new DateTime(now.Year, now.Month, now.Day, 0, 0, 0)
                    .AddDays(1 - bucketCount),

            "6m" or "1y"
                => new DateTime(now.Year, now.Month, 1, 0, 0, 0)
                    .AddMonths(1 - bucketCount),

            "24h" or _
                => new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)
                    .AddHours(1 - bucketCount),
        };

        var baseQuery = _kriRepo
            .GetAllReadings()
            .Where(r => from <= r.Timestamp)
            .Select(r => new { Timestamp = r.Timestamp, Value = r.Value });

        var scores = trendTimeFrame switch
        {
            "7d" or "30d" or "90d" => await baseQuery
                    .GroupBy(r => new { Year = r.Timestamp.Year, Month = r.Timestamp.Month, Day = r.Timestamp.Day })
                    .ToDictionaryAsync(
                        g => new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                        g => g.Average(r => r.Value)
                    ),

            "6m" or "1y" => await baseQuery
                    .GroupBy(r => new { Year = r.Timestamp.Year, Month = r.Timestamp.Month })
                    .ToDictionaryAsync(
                        g => new DateTime(g.Key.Year, g.Key.Month, 1),
                        g => g.Average(r => r.Value)
                    ),

            "24h" or _ => await baseQuery
                .GroupBy(r => new { Year = r.Timestamp.Year, Month = r.Timestamp.Month, Day = r.Timestamp.Day, Hour = r.Timestamp.Hour })
                .ToDictionaryAsync(
                    g => new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    g => g.Average(r => r.Value)
                ),
        };

        Func<DateTime, string> dateToStr = trendTimeFrame switch
        {
            "7d" or "30d" or "90d" => (dt) => dt.ToLocalTime().ToString("MMM dd"),
            "6m" or "1y" => (dt) => dt.ToLocalTime().ToString("yyyy MMM"),
            "24h" or _ => (dt) => dt.ToLocalTime().ToString("HH:mm"),
        };

        var buckets = trendTimeFrame switch
        {
            "7d" or "30d" or "90d" => Enumerable.Range(0, bucketCount).Select(b => from.AddDays(b)),
            "6m" or "1y" => Enumerable.Range(0, bucketCount).Select(b => from.AddMonths(b)),
            "24h" or _ => Enumerable.Range(0, bucketCount).Select(b => from.AddHours(b)),
        };

        return buckets
            .Select(b => new DataPoint
            {
                Label = dateToStr(b),
                Value = scores.GetValueOrDefault(b, 0.0),
            });
    }
}
