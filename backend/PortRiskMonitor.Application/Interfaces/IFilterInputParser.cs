using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.Interfaces;

public interface IFilterInputParser
{
    (DateTime from, DateTime to) ParseFilterInput(string preset, string? fromStr, string? toStr);
    (DateTime from, int bucketCount, BucketType interval) ParseFilterInput(string trendTimeFrame);
}
