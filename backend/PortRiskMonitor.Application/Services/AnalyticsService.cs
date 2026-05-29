using Microsoft.Extensions.Configuration;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IConfiguration _config;
    private readonly IFilterInputParser _filterInputParser;
    private readonly IRiskMonitorRepository _portRiskMonitorRepo;

    public AnalyticsService(
        IConfiguration config,
        IFilterInputParser filterInputParser,
        IRiskMonitorRepository portRiskMonitorRepo)
    {
        _config = config;
        _filterInputParser = filterInputParser;
        _portRiskMonitorRepo = portRiskMonitorRepo;
    }

    public async Task<AnalyticsDto> GetAnalytics(string slug, string? fromStr, string? toStr)
    {
        var (from, to) = _filterInputParser.ParseFilterInput("custom", fromStr, toStr);

        var desiredNumberOfPoints = _config.GetValue<long>("Graphing:DesiredNumberOfPoints:AnalyticsCard", 500);
        var bucketLength = (to - from) / (desiredNumberOfPoints / 4);

        var kri = await _portRiskMonitorRepo.GetBySlugAsync(slug)
            ?? throw new NotFoundException("Risk indicator not found");

        var scores = await _portRiskMonitorRepo
            .GetKriReadings(slug)
            .Where(r => from <= r.Timestamp && r.Timestamp <= to)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .DownsampleM4Async(from, bucketLength);

        return new AnalyticsDto(
            Title: kri.Name,
            GreenMax: kri.GreenMax,
            YellowMax: kri.YellowMax,
            Sparkline: scores
                .Select(b => new DataPoint
                {
                    Label = b.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Value = b.Value,
                })
                .ToArray()
        );
    }
}
