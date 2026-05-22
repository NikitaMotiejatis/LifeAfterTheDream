using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;
using RiskMonitor.Services;

namespace PortRiskMonitor.Application.Services;

public class PortRiskMonitorService : RiskMonitorService, IPortRiskMonitorService
{
    private readonly IRiskMonitorRepository _portRiskMonitorRepo;

    public PortRiskMonitorService(IRiskMonitorRepository portRiskMonitorRepo)
        : base(portRiskMonitorRepo)
    {
        _portRiskMonitorRepo = portRiskMonitorRepo;
    }

    public async Task<AnalyticsDto> GetAnalytics(string slug, string? from, string? to)
    {
        var (fromDateTime, toDateTime, bucketCount, interval) = ParseFilterInput("", from, to);
        var outputDateTimeFormat = (toDateTime - fromDateTime).Ticks switch
        {
            > 10 * 365 * TimeSpan.TicksPerDay => "yyyy",
            > 5 * 30 * TimeSpan.TicksPerDay => "MM/yyyy",
            > 3 * TimeSpan.TicksPerDay => "dd/MM",
            _ => "HH:mm",
        };

        var kri = await _riskMonitorRepo
            .GetAllIndicators()
            .Where(kri => kri.Slug == slug)
            .FirstOrDefaultAsync()
            ?? throw new Exception("Not found");

        var readings = await _riskMonitorRepo
            .GetKriReadings(slug)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .BucketScores(fromDateTime, bucketCount, interval);

        return new AnalyticsDto(
            Title: kri.Name,
            GreenMax: kri.GreenMax,
            YellowMax: kri.YellowMax,
            Sparkline: readings
                .Select(b => new DataPoint
                {
                    Label = b.Timestamp.ToLocalTime().ToString(outputDateTimeFormat),
                    Value = b.Value,
                })
                .ToArray()
        );
    }

    public async Task<IEnumerable<KriCardDto>> GetKriCards(string preset, string? from, string? to)
    {
        var (fromDateTime, toDateTime, bucketCount, interval) = ParseFilterInput(preset, from, to);
        var outputDateTimeFormat = (toDateTime - fromDateTime).Ticks switch
        {
            > 10 * 365 * TimeSpan.TicksPerDay => "yyyy",
            > 5 * 30 * TimeSpan.TicksPerDay => "MM/yyyy",
            > 3 * TimeSpan.TicksPerDay => "dd/MM",
            _ => "HH:mm",
        };

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
                    .Where(r => fromDateTime <= r.Timestamp && r.Timestamp <= toDateTime)
                    .Select(r => new ScoreInfo
                    {
                        Timestamp = r.Timestamp,
                        Value = r.Value,
                    })
                    .BucketScoresEnumerable(fromDateTime, bucketCount, interval)
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
                        Label = r.Timestamp.ToLocalTime().ToString(outputDateTimeFormat),
                        Value = r.Value,
                    })
                    .ToArray()
            ));
    }

    private (DateTime from, DateTime to, int bucketCount, BucketType interval) ParseFilterInput(string preset, string? from, string? to)
    {
        var now = DateTime.UtcNow;
        var parseFormat = "yyyy-MM-ddTHH:mm";
        var provider = CultureInfo.InvariantCulture;

        var displayFormat = preset switch
        {
            "6h" or "12h" or "24h" => "HH:mm",
            "year" => "MM/yy",
            _ => "dd/MM",
        };

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
            fromDateTime = DateTime.ParseExact(from ?? "", parseFormat, provider);
            toDateTime = DateTime.ParseExact(to ?? "", parseFormat, provider);
        }
#pragma warning disable CS0168
        catch (Exception e) { }
#pragma warning restore CS0168

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
