using PortRiskMonitor.Application.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IIndicatorService
{
    Task<NewKriReadingDto> AddReading(string slug, NewKriReadingDto readingCreationInfo);
}
