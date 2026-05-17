// ============================================================
// SeedData.cs — Development seed data
//
// Seeds 4 Kri definitions + 30 days of hourly KriReadings on first startup.
// Idempotent: skips if any Kris already exist (AnyAsync guard).
// Called from Program.cs after MigrateAsync().
//
// To reset: delete the .db file and restart.
// ============================================================

using Microsoft.EntityFrameworkCore;
using RiskMonitor.Entities;

namespace PortRiskMonitor.Infrastructure.Data;

public static class SeedData
{
    // Slugs used by GET /api/history/{slug}
    public const string PortStatusSlug = "port-status";
    public const string BerthSlug = "berth";
    public const string VesselDelaySlug = "vessel-delays";
    public const string WeatherSlug = "weather";
    public const string CustomsSlug = "customs";

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Kris.AnyAsync()) return;

        var now = DateTime.UtcNow;
        var from = now.AddYears(-1);

        // ── KRI definitions ────────────────────────────────────────────────────
        var kris = new List<Kri>
        {
            new()
            {
                Id           = Guid.NewGuid(),
                Name         = "Port Disruption Index",
                Description  = "Index encompassing all other indicators to show combined disruption level.",
                Unit         = "",
                Slug         = PortStatusSlug,
                MockBaseline = 50,
                MockVariance = 50,
                MockPattern  = "Sinusoidal",
                CreatedAt    = now,
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Name         = "Berth Occupancy Rate",
                Description  = "Percentage of berths currently occupied by vessels.",
                Unit         = "%",
                Slug         = BerthSlug,
                MockBaseline = 60,
                MockVariance = 30,
                MockPattern  = "Sinusoidal",
                CreatedAt    = now,
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Name         = "Vessel Delay Rate",
                Description  = "Percentage of scheduled vessels with a delayed arrival.",
                Unit         = "%",
                Slug         = VesselDelaySlug,
                MockBaseline = 22,
                MockVariance = 18,
                MockPattern  = "RandomWalk",
                CreatedAt    = now,
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Name         = "Weather Condition Score",
                Description  = "Composite weather risk score (wind speed, water level, condition code).",
                Unit         = "",
                Slug         = WeatherSlug,
                MockBaseline = 18,
                MockVariance = 28,
                MockPattern  = "Sinusoidal",
                CreatedAt    = now,
            },
            new()
            {
                Id           = Guid.NewGuid(),
                Name         = "Customs Dwell Time",
                Description  = "Average hours cargo spends in customs clearance.",
                Unit         = " h",
                Slug         = CustomsSlug,
                MockBaseline = 18,
                MockVariance = 40,
                MockPattern  = "StepFunction",
                CreatedAt    = now,
            },
        };

        await db.Kris.AddRangeAsync(kris);

        var rng = new Random(42); // fixed seed = reproducible dev data
        var readings = new List<KriReading>(kris.Count * 2 * 24 * 365);

        foreach (var kri in kris)
            readings.AddRange(GenerateReadings(kri, from, now, rng));

        await db.KriReadings.AddRangeAsync(readings);
        await db.SaveChangesAsync();
    }

    // ── Generator ──────────────────────────────────────────────────────────────

    private static IEnumerable<KriReading> GenerateReadings(
        Kri kri, DateTime from, DateTime to, Random rng)
    {
        var readings = new List<KriReading>();
        var cursor = from;
        var walkValue = kri.MockBaseline;

        while (cursor <= to)
        {
            var raw = kri.MockPattern switch
            {
                "Sinusoidal" => Sinusoidal(kri, cursor, rng),
                "RandomWalk" => RandomWalk(ref walkValue, kri, rng),
                "StepFunction" => StepFunction(kri, cursor, rng),
                _ => Sinusoidal(kri, cursor, rng),
            };

            var value = Math.Round(Math.Clamp(raw, 0, 120), 2);

            readings.Add(new KriReading
            {
                Id = Guid.NewGuid(),
                KriId = kri.Id,
                Value = value,
                Timestamp = cursor,
            });

            cursor = cursor.AddHours(1);
        }

        return readings;
    }

    /// <summary>
    /// Peaks at shift-change hours (06:00 and 18:00 UTC).
    /// </summary>
    private static double Sinusoidal(Kri kri, DateTime t, Random rng)
    {
        var cycle = Math.Sin(2 * Math.PI * t.Hour / 24.0) * (kri.MockVariance * 0.55);
        var noise = (rng.NextDouble() - 0.5) * kri.MockVariance * 0.45;
        return kri.MockBaseline + cycle + noise;
    }

    /// <summary>
    /// Drifts randomly with mean-reversion toward the baseline.
    /// </summary>
    private static double RandomWalk(ref double current, Kri kri, Random rng)
    {
        current += (rng.NextDouble() - 0.5) * kri.MockVariance * 0.25;
        current += (kri.MockBaseline - current) * 0.03;
        return current;
    }

    /// <summary>
    /// Holds baseline, spikes on Mon/Thu inspection days and Friday evening backlog.
    /// </summary>
    private static double StepFunction(Kri kri, DateTime t, Random rng)
    {
        var isInspection = t.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday
                              && t.Hour is >= 8 and <= 18;
        var isWeekendBacklog = t.DayOfWeek == DayOfWeek.Friday && t.Hour >= 16;

        var level = isInspection ? kri.MockBaseline + kri.MockVariance * 0.7
                  : isWeekendBacklog ? kri.MockBaseline + kri.MockVariance * 0.9
                  : kri.MockBaseline;

        return level + (rng.NextDouble() - 0.5) * kri.MockVariance * 0.15;
    }
}
