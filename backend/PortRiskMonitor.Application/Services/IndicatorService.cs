using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.Entities;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class IndicatorService : IIndicatorService
{
    private readonly IRiskMonitorRepository _riskMonitorRepo;

    public IndicatorService(IRiskMonitorRepository riskMonitorRepo)
    {
        _riskMonitorRepo = riskMonitorRepo;
    }

    public async Task<NewKriReadingDto> AddReading(string slug, NewKriReadingDto readingCreationInfo)
    {
        var kri = await _riskMonitorRepo.GetBySlugAsync(slug)
            ?? throw new BadInputException($"Could not find Kri with slug {slug}");

        var writtenReading = await _riskMonitorRepo.AddReadingAsync(new KriReading
        {
            Value = readingCreationInfo.Value,
            Timestamp = readingCreationInfo.Timestamp,
            KriId = kri.Id,
            Kri = kri,
        });

        return new NewKriReadingDto
        {
            Value = writtenReading.Value,
            Timestamp = writtenReading.Timestamp,
        };
    }
}
