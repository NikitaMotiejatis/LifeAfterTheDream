using Microsoft.EntityFrameworkCore;

using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IFilterInputParser _filterInputParser;
    private readonly IRiskMonitorRepository _portRiskMonitorRepo;

    public AnalyticsService(
        IFilterInputParser filterInputParser,
        IRiskMonitorRepository portRiskMonitorRepo)
    {
        _filterInputParser = filterInputParser;
        _portRiskMonitorRepo = portRiskMonitorRepo;
    }

    public async Task<AnalyticsDto> GetAnalytics(string slug, string? fromStr, string? toStr)
    {
        var (from, to) = _filterInputParser.ParseFilterInput("custom", fromStr, toStr);

        const long numberOfBuckets = 50;
        var bucketLength = (to - from) / numberOfBuckets;

        var kri = await _portRiskMonitorRepo
            .GetAllIndicators()
            .Where(kri => kri.Slug == slug)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Risk indicator not found");

        var readings = await _portRiskMonitorRepo
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
}
