using Microsoft.EntityFrameworkCore;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.PortStatus;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class DashboardService : IDasboardService
{
    private readonly IRiskMonitorRepository _riskMonitorRepo;
    private readonly IPortStatusRepo _portStatusRepo;
    private readonly IWeatherSnapshotCache _weatherCache;

    public DashboardService(
            IRiskMonitorRepository riskMonitorRepo,
            IPortStatusRepo portStatusRepo,
            IWeatherSnapshotCache weatherCache)
    {
        _riskMonitorRepo = riskMonitorRepo;
        _portStatusRepo = portStatusRepo;
        _weatherCache = weatherCache;
    }

    public async Task<PortStatusDto> GetPortStatus(string preset, string? fromStr, string? toStr)
    {
        var (from, to) = ParseFilterInput(preset, fromStr, toStr);

        const long numberOfBuckets = 30;
        var bucketLength = (to - from) / numberOfBuckets;

        var scores = await _portStatusRepo
            .GetReadings(from, to)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .DownsampleM4Async(from, 4 * bucketLength);

        var disruptionIndex = (await _portStatusRepo.GetLatestReading())?.Value;
        var kri = await _portStatusRepo.GetKri();

        return new PortStatusDto
        {
            DisruptionIndex = disruptionIndex ?? 0.0,
            RiskLevel = disruptionIndex <= kri.GreenMax ? "Low" : disruptionIndex <= kri.YellowMax ? "Moderate" : "High",
            GreenMax = kri.GreenMax,
            YellowMax = kri.YellowMax,
            Sparkline = scores
                .Select(s => new DataPoint
                {
                    Label = s.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Value = s.Value,
                })
                .ToArray(),
        };
    }

    public Task<WeatherSnapshot> GetWeather()
        => Task.FromResult(_weatherCache.GetLatest());

    public async Task<IEnumerable<KriCardDto>> GetKriCards(string preset, string? fromStr, string? toStr)
    {
        var (from, to) = ParseFilterInput(preset, fromStr, toStr);

        const long numberOfBuckets = 30;
        var bucketLength = (to - from) / numberOfBuckets;

        var krisWithReadings = await _riskMonitorRepo
            .GetAllIndicators()
            .Where(kri => kri.Slug != "port-status")
            .Select(kri => new
            {
                Slug = kri.Slug,
                Name = kri.Name,
                Unit = kri.Unit,
                GreenMax = kri.GreenMax,
                YellowMax = kri.YellowMax,
                LatestReading = kri.Readings
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefault(),
                Scores = kri.Readings
                    .Where(r => from <= r.Timestamp && r.Timestamp <= to)
                    .Select(r => new ScoreInfo
                    {
                        Timestamp = r.Timestamp,
                        Value = r.Value,
                    })
                    .DownsampleM4Enumerable(from, 4 * bucketLength)
                    .ToArray(),
            })
            .ToArrayAsync();

        return krisWithReadings
            .Select(kri => new KriCardDto(
                Id: kri.Slug,
                Title: kri.Name,
                Value: (string.Format("{0:0.0}", kri.LatestReading?.Value) + kri.Unit) ?? "Not Available",
                Formula: "",
                Thresholds: BuildThresholds(kri.GreenMax, kri.YellowMax, kri.Unit),
                Severity: GetSeverity(kri.LatestReading?.Value, kri.GreenMax, kri.YellowMax),
                GreenMax: kri.GreenMax,
                YellowMax: kri.YellowMax,
                Sparkline: kri.Scores
                    .Select(r => new DataPoint
                    {
                        Label = r.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        Value = r.Value,
                    })
                    .ToArray()
            ));
    }

    public async Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame)
    {
        var (from, bucketCount, interval) = ParseFilterInput(trendTimeFrame);

        var scores = await _portStatusRepo
            .GetAllReadings()
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .DownsampleAverageAsync(from, bucketCount, interval);

        return scores
            .Select(bucket => new DataPoint
            {
                Label = bucket.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Value = bucket.Value,
            });
    }

    private static (DateTime from, DateTime to) ParseFilterInput(string preset, string? fromStr, string? toStr)
    {
        var from = DateTime.MinValue;
        var to = DateTime.UtcNow;

        if (preset == "custom")
        {
            if (fromStr is not null && !DateTime.TryParse(fromStr, out from))
                throw new BadInputException("Failed to parse 'from' filter option");

            if (toStr is not null && !DateTime.TryParse(toStr, out to))
                throw new BadInputException("Failed to parse 'to' filter option");

            return (from.ToUniversalTime(), to);
        }

        from = preset switch
        {
            "6h" => to.AddHours(-6),
            "12h" => to.AddHours(-12),
            "24h" => to.AddHours(-24),
            "48h" => to.AddHours(-48),
            "72h" => to.AddHours(-72),
            "week" => to.AddDays(-7),
            "month" => to.AddMonths(-1),
            "year" => to.AddYears(-1),
            _ => throw new BadInputException("Invalid data filter 'preset'"),
        };

        return (from, to);
    }

    private static (DateTime from, int bucketCount, BucketType interval) ParseFilterInput(string trendTimeFrame)
    {
        var now = DateTime.UtcNow;

        var (bucketCount, interval) = trendTimeFrame switch
        {
            "24h" => (24, BucketType.Hour),
            "7d" => (7, BucketType.Day),
            "30d" => (30, BucketType.Day),
            "90d" => (90, BucketType.Day),
            "6m" => (6, BucketType.Month),
            "1y" => (12, BucketType.Month),
            _ => throw new BadInputException("Invalid trend timeframe"),
        };

        var from = interval switch
        {
            BucketType.Hour => (new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)).AddHours(1 - bucketCount),
            BucketType.Day => (new DateTime(now.Year, now.Month, now.Day)).AddDays(1 - bucketCount),
            BucketType.Month => (new DateTime(now.Year, now.Month, 1)).AddMonths(1 - bucketCount),
            BucketType.Year => (new DateTime(now.Year, 1, 1)).AddYears(1 - bucketCount),
            _ => throw new InternalErrorException("Invalid time interval length"),
        };

        return (from, bucketCount, interval);
    }

    private static ThresholdDto[] BuildThresholds(double greenMax, double yellowMax, string unit)
    {
        var u = unit.Trim();
        return new[]
        {
            new ThresholdDto($"<{greenMax}{u}", "#22c55e", "Low"),
            new ThresholdDto($"{greenMax}-{yellowMax}{u}", "#eab308", "Medium"),
            new ThresholdDto($">{yellowMax}{u}", "#ef4444", "High"),
        };
    }

    private static string GetSeverity(double? value, double greenMax, double yellowMax)
    {
        if (!value.HasValue) return "Low";
        if (value.Value <= greenMax) return "Low";
        if (value.Value <= yellowMax) return "Medium";
        return "High";
    }
}
