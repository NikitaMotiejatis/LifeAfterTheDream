using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using PortRiskMonitor.Data.PortStatus;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class DashboardService : IDasboardService
{
    private readonly IConfiguration _config;
    private readonly IFilterInputParser _filterInputParser;

    private readonly IRiskMonitorRepository _riskMonitorRepo;
    private readonly IPortStatusRepo _portStatusRepo;

    private readonly IWeatherSnapshotCache _weatherCache;
    private readonly IAisSnapshotCache _aisCache;

    public DashboardService(
            IConfiguration config,
            IFilterInputParser filterInputParser,
            IRiskMonitorRepository riskMonitorRepo,
            IPortStatusRepo portStatusRepo,
            IWeatherSnapshotCache weatherCache,
            IAisSnapshotCache aisCache)
    {
        _config = config;
        _filterInputParser = filterInputParser;
        _riskMonitorRepo = riskMonitorRepo;
        _portStatusRepo = portStatusRepo;
        _weatherCache = weatherCache;
        _aisCache = aisCache;
    }

    public async Task<PortStatusDto> GetPortStatus(string preset, string? fromStr, string? toStr)
    {
        var (from, to) = _filterInputParser.ParseFilterInput(preset, fromStr, toStr);

        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var desiredNumberOfPoints = _config.GetValue<long>("Graphing:DesiredNumberOfPoints:PortStatusMini", 100);
        var bucketLength = (to - from) / (desiredNumberOfPoints / 4);

        var scores = await _portStatusRepo
            .GetScores(from, to)
            .Where(r => from <= r.Timestamp && r.Timestamp <= to)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .DownsampleM4Async(from, bucketLength);

        var disruptionIndex = (await _portStatusRepo.GetLatestReading())?.Value;
        var kri = await _portStatusRepo.GetKriWithReadings(from, to)
            ?? throw new InternalErrorException("Could not find Kri");

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
        var (from, to) = _filterInputParser.ParseFilterInput(preset, fromStr, toStr);
        var now = DateTime.UtcNow;

        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var desiredNumberOfPoints = _config.GetValue<long>("Graphing:DesiredNumberOfPoints:KriCard", 300);
        var bucketLength = (to - from) / (desiredNumberOfPoints / 4);

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
                    .Where(r => r.Timestamp <= now)
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefault(),
                Scores = kri.Readings
                    .Where(r => from <= r.Timestamp && r.Timestamp <= to)
                    .Select(r => new ScoreInfo
                    {
                        Timestamp = r.Timestamp,
                        Value = r.Value,
                    })
                    .DownsampleM4(from, bucketLength),
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
        var (from, bucketCount, interval) = _filterInputParser.ParseFilterInput(trendTimeFrame);
        var to = DateTime.UtcNow;

        from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        var scores = await _portStatusRepo
            .GetScores(from, to)
            .DownsampleAverageAsync(from, bucketCount, interval);

        return scores
            .Select(bucket => new DataPoint
            {
                Label = bucket.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Value = bucket.Value,
            });
    }

    public Task<AisSnapshot> GetAis()
        => Task.FromResult(_aisCache.GetLatest());

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
