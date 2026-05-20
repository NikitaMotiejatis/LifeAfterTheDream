using System.Globalization;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Infrastructure.PortStatus;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
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
        var (fromDateTime, toDateTime, bucketCount, interval) = ParseFilterInput(preset, from, to);
        var outputDateTimeFormat = (toDateTime - fromDateTime).Ticks switch
        {
            > 10 * 365 * TimeSpan.TicksPerDay => "yyyy",
            > 5 * 30 * TimeSpan.TicksPerDay => "MM/yyyy",
            > 3 * TimeSpan.TicksPerDay => "dd/MM",
            _ => "HH:mm",
        };

        var disruptionIndex = await GetLatestScore() ?? 0.0;
        var kri = await _portStatusRepo.GetKri();

        var scores = await _kriRepo
            .GetAllReadings()
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .BucketScores(fromDateTime, bucketCount, interval);

        var sparkline = scores
            .Select(s => new PortStatusDto.SparkPoint
            {
                Label = s.Timestamp.ToLocalTime().ToString(outputDateTimeFormat),
                Value = s.Value,
            }).ToList();

        return new PortStatusDto
        {
            DisruptionIndex = disruptionIndex,
            RiskLevel = disruptionIndex <= kri.GreenMax ? "Low" : disruptionIndex <= kri.YellowMax ? "Moderate" : "High",
            GreenMax = kri.GreenMax,
            YellowMax = kri.YellowMax,
            Sparkline = sparkline,
        };
    }

    public async Task<IEnumerable<DataPoint>> GetTrend(string trendTimeFrame)
    {
        var (from, bucketCount, interval) = ParseFilterInput(trendTimeFrame);
        var outputDateTimeFormat = interval switch
        {
            BucketType.Hour => "HH:mm",
            BucketType.Day => "dd/MM",
            BucketType.Month => "MM/yyyy",
            BucketType.Year => "yyyy",
            _ => throw new InternalErrorException("Invalid time interval length"),
        };

        var scores = await _kriRepo
            .GetAllReadings()
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .BucketScores(from, bucketCount, interval);

        return scores
            .Select(bucket => new DataPoint
            {
                Label = bucket.Timestamp.ToLocalTime().ToString(outputDateTimeFormat),
                Value = bucket.Value,
            });
    }

    private (DateTime from, DateTime to, int bucketCount, BucketType interval) ParseFilterInput(string preset, string? from, string? to)
    {
        var now = DateTime.UtcNow;
        var parseFormat = "yyyy-MM-ddTHH:mm";
        var provider = CultureInfo.InvariantCulture;

        DateTime fromDateTime = DateTime.MinValue;
        DateTime toDateTime = DateTime.MaxValue;

        if (from is not null && to is not null)
        {
            try
            {
                fromDateTime = DateTime.ParseExact(from ?? "", parseFormat, provider);
                toDateTime = DateTime.ParseExact(to ?? "", parseFormat, provider);
            }
#pragma warning disable CS0168
            catch (Exception e)
            {
#pragma warning restore CS0168
                throw new BadInputException("Invalid data filter 'from' and/or 'to' date");
            }
        }
        else
        {
            fromDateTime = preset switch
            {
                "6h" => now.AddHours(-6),
                "12h" => now.AddHours(-12),
                "24h" => now.AddHours(-24),
                "48h" => now.AddHours(-48),
                "72h" => now.AddHours(-72),
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                "year" => now.AddYears(-1),
                _ => throw new BadInputException("Invalid data filter 'preset'"),
            };
            toDateTime = now;
        }

        var totalInterval = toDateTime - fromDateTime;
        var (bucketCount, interval) = totalInterval.Ticks switch
        {
            > 30 * 365 * TimeSpan.TicksPerDay => (totalInterval.Ticks / (365 * TimeSpan.TicksPerDay), BucketType.Year),
            > 365 * TimeSpan.TicksPerDay => (totalInterval.Ticks / (30 * TimeSpan.TicksPerDay), BucketType.Month),
            > 21 * TimeSpan.TicksPerDay => (totalInterval.Ticks / TimeSpan.TicksPerDay, BucketType.Day),
            _ => (totalInterval.Ticks / TimeSpan.TicksPerHour, BucketType.Hour),
        };

        return (fromDateTime, toDateTime, (int)Math.Min(bucketCount, (long)Int32.MaxValue), interval);
    }

    private (DateTime from, int bucketCount, BucketType interval) ParseFilterInput(string trendTimeFrame)
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

}
