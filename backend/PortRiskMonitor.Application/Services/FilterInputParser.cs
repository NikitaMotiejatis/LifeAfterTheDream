using PortRiskMonitor.Application.Exceptions;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.DTOs;

namespace PortRiskMonitor.Application.Services;

public class FilterInputParser : IFilterInputParser
{
    public (DateTime from, DateTime to) ParseFilterInput(string preset, string? fromStr, string? toStr)
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

    public (DateTime from, int bucketCount, BucketType interval) ParseFilterInput(string trendTimeFrame)
    {
        var now = DateTime.UtcNow;

        var (bucketCount, interval) = trendTimeFrame switch
        {
            "24h" => (24, BucketType.Hour),
            "7d" => (7, BucketType.Day),
            "30d" => (30, BucketType.Day),
            "90d" => (90, BucketType.Day),
            "6m" => (6, BucketType.Month),
            "1y" => (12, BucketType.Month),
            _ => throw new BadInputException("Invalid trend timeframe"),
        };

        var from = interval switch
        {
            BucketType.Hour => (new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0)).AddHours(1 - bucketCount),
            BucketType.Day => (new DateTime(now.Year, now.Month, now.Day)).AddDays(1 - bucketCount),
            BucketType.Month => (new DateTime(now.Year, now.Month, 1)).AddMonths(1 - bucketCount),
            BucketType.Year => (new DateTime(now.Year, 1, 1)).AddYears(1 - bucketCount),
            _ => throw new InternalErrorException("Invalid time interval length"),
        };

        return (from, bucketCount, interval);
    }
}
