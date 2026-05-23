using System.Data;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
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

    public async Task<AnalyticsDto> GetAnalytics(string slug, string? fromStr, string? toStr)
    {
        var (from, to) = ParseFilterInput("custom", fromStr, toStr);

        const long numberOfBuckets = 50;
        var bucketLength = (to - from) / numberOfBuckets;

        var kri = await _riskMonitorRepo
            .GetAllIndicators()
            .Where(kri => kri.Slug == slug)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Risk indicator not found");

        var readings = await _riskMonitorRepo
            .GetKriReadings(slug)
            .Where(r => from <= r.Timestamp && r.Timestamp <= to)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .DownsampleM4Async(from, 4 * bucketLength);

        return new AnalyticsDto(
            Title: kri.Name,
            GreenMax: kri.GreenMax,
            YellowMax: kri.YellowMax,
            Sparkline: readings
                .Select(b => new DataPoint
                {
                    Label = b.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Value = b.Value,
                })
                .ToArray()
        );
    }

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

    private (DateTime from, DateTime to) ParseFilterInput(string preset, string? fromStr, string? toStr)
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
