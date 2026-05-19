using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;
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

    public async Task<IEnumerable<KriCardDto>> GetKriCards(string preset, string? from, string? to)
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

        var rawData = await GetKrisWithReadings(fromDateTime, toDateTime, (kri) => kri.Slug != "port-status")
            .Select(x => new
            {
                Slug = x.Kri.Slug,
                Name = x.Kri.Name,
                Unit = x.Kri.Unit,
                LatestValue = x.Readings
                    .OrderByDescending(r => r.Timestamp)
                    .Select(r => (double?)r.Value)
                    .FirstOrDefault(),
                SparklineData = x.Readings
                    .Select(r => new { r.Timestamp, r.Value })
                    .ToList()
            }).OrderBy(x => x.Slug)
            .ToListAsync();

        return rawData
            .Select(x => new KriCardDto(
                Id: x.Slug,
                Title: x.Name,
                Value: x.LatestValue.HasValue
                    ? $"{String.Format("{0:0.0}", x.LatestValue.Value)}{x.Unit}"
                    : "Not Available",
                Sparkline: x.SparklineData
                    .Select(s => new DataPoint
                    {
                        Label = s.Timestamp.ToLocalTime().ToString(format, provider),
                        Value = s.Value,
                    }).ToList()
            ));
    }
}
