using System.Globalization;
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

        const string format = "yyyy-MM-ddTHH:mm:ss.fff'Z'";

        if (preset == "custom")
        {
            if (fromStr is not null && !DateTime.TryParse(fromStr, out from))
                throw new BadInputException("Failed to parse 'from' filter option");

            if (fromStr is not null
                    && !DateTime.TryParseExact(
                        fromStr,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out from))
                throw new BadInputException("Failed to parse 'from' filter option");

            if (toStr is not null
                    && !DateTime.TryParseExact(
                        toStr,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out to))
                throw new BadInputException("Failed to parse 'to' filter option");

            to = DateTime.UtcNow < to ? DateTime.UtcNow : to;

            return (DateTime.SpecifyKind(from, DateTimeKind.Utc),
                    DateTime.SpecifyKind(to, DateTimeKind.Utc));
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

        return (DateTime.SpecifyKind(from, DateTimeKind.Utc),
                DateTime.SpecifyKind(to, DateTimeKind.Utc));
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
            BucketType.Hour => (new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc)).AddHours(1 - bucketCount),
            BucketType.Day => (new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc)).AddDays(1 - bucketCount),
            BucketType.Month => (new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)).AddMonths(1 - bucketCount),
            BucketType.Year => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddYears(1 - bucketCount),
            _ => throw new InternalErrorException("Invalid time interval length"),
        };

        return (DateTime.SpecifyKind(from, DateTimeKind.Utc), bucketCount, interval);
    }
}
