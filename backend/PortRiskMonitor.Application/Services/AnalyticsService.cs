using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.DTOs;
using RiskMonitor.Extensions;
using RiskMonitor.Repositories;
using Microsoft.EntityFrameworkCore;

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
        var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

        const long numberOfBuckets = 50;
        var bucketLength = (toUtc - fromUtc) / numberOfBuckets;

        var kri = await _portRiskMonitorRepo.GetBySlugAsync(slug)
            ?? throw new NotFoundException("Risk indicator not found");

        var scores = (await _portRiskMonitorRepo
            .GetKriReadings(slug)
            .Where(r => fromUtc <= r.Timestamp && r.Timestamp <= toUtc)
            .Select(r => new ScoreInfo
            {
                Timestamp = r.Timestamp,
                Value = r.Value,
            })
            .ToListAsync())
            .AsEnumerable()
            .DownsampleM4Enumerable(fromUtc, 4 * bucketLength);

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
