// ============================================================
// SeedData.cs — Development seed: KRI definitions + 30 days of history
//
// Called once from Program.cs on startup (idempotent — skips if data exists).
// Uses the same EF Core / AppDbContext that everything else uses, so swapping
// to Postgres later requires zero changes here — only the connection string
// and UseSqlite → UseNpgsql in Program.cs need to change.
//
// Indicator slugs are stored in KriDefinition.FormulaLabel so the
// History controller can look them up without an extra migration.
//
// Pattern options per KRI:
//   Sinusoidal   — daily peaks at shift-change hours (06:00 / 18:00 UTC)
//   RandomWalk   — gradual drift that reverts toward the baseline
//   StepFunction — holds steady, jumps on inspection days / weekend backlog
// ============================================================

using Microsoft.EntityFrameworkCore;
using PortRiskMonitor.Infrastructure.Entities;

namespace PortRiskMonitor.Infrastructure.Data;

public static class SeedData
{
    // These names are used by the History controller to map slugs → KriDefinitionId.
    public const string BerthSlug = "berth";
    public const string VesselDelaySlug = "vessel-delays";
    public const string WeatherSlug = "weather";
    public const string CustomsSlug = "customs";

    public static async Task SeedAsync(AppDbContext db)
    {
        // Idempotent — only seed once
        if (await db.KriDefinitions.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var from = now.AddDays(-30);

        // ── KRI Definitions ───────────────────────────────────────────────────
        // FormulaLabel = URL slug used by GET /api/history/{slug}
        var definitions = new List<KriDefinition>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name            = "Berth Occupancy Rate",
                Unit            = "%",
                Description     = "Percentage of berths currently occupied by vessels.",
                FormulaLabel    = BerthSlug,
                GreenMax        = 70,
                YellowMax       = 90,
                Weight          = 0.25,
                HigherIsWorse   = true,
                MockBaseline    = 60,
                MockVariance    = 30,
                MockPattern     = "Sinusoidal",
                CreatedAt       = now,
                UpdatedAt       = now,
                RowVersion      = new byte[8],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name            = "Vessel Delay Rate",
                Unit            = "%",
                Description     = "Percentage of scheduled vessels with a delayed arrival.",
                FormulaLabel    = VesselDelaySlug,
                GreenMax        = 20,
                YellowMax       = 40,
                Weight          = 0.25,
                HigherIsWorse   = true,
                MockBaseline    = 22,
                MockVariance    = 18,
                MockPattern     = "RandomWalk",
                CreatedAt       = now,
                UpdatedAt       = now,
                RowVersion      = new byte[8],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name            = "Weather Condition Score",
                Unit            = "score",
                Description     = "Composite weather risk score (wind speed, water level, condition code).",
                FormulaLabel    = WeatherSlug,
                GreenMax        = 33,
                YellowMax       = 66,
                Weight          = 0.25,
                HigherIsWorse   = true,
                MockBaseline    = 18,
                MockVariance    = 28,
                MockPattern     = "Sinusoidal",
                CreatedAt       = now,
                UpdatedAt       = now,
                RowVersion      = new byte[8],
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name            = "Customs Dwell Time",
                Unit            = "hours",
                Description     = "Average hours cargo spends in customs clearance.",
                FormulaLabel    = CustomsSlug,
                GreenMax        = 24,
                YellowMax       = 72,
                Weight          = 0.25,
                HigherIsWorse   = true,
                MockBaseline    = 18,
                MockVariance    = 40,
                MockPattern     = "StepFunction",
                CreatedAt       = now,
                UpdatedAt       = now,
                RowVersion      = new byte[8],
            },
        };

        await db.KriDefinitions.AddRangeAsync(definitions);

        // ── Historical readings — 30 days × 1 reading/hour ───────────────────
        // Fixed seed = same data every first run (good for deterministic dev/tests)
        var rng = new Random(42);
        var readings = new List<KriReading>(definitions.Count * 24 * 30);

        foreach (var kri in definitions)
            readings.AddRange(GenerateReadings(kri, from, now, rng));

        await db.KriReadings.AddRangeAsync(readings);
        await db.SaveChangesAsync();
    }

    // ── Reading generator ─────────────────────────────────────────────────────

    private static IEnumerable<KriReading> GenerateReadings(
        KriDefinition kri, DateTime from, DateTime to, Random rng)
    {
        var readings = new List<KriReading>();
        var cursor = from;
        var walkValue = kri.MockBaseline; // mutable state for RandomWalk

        while (cursor <= to)
        {
            var raw = kri.MockPattern switch
            {
                "Sinusoidal"   => Sinusoidal(kri, cursor, rng),
                "RandomWalk"   => RandomWalk(ref walkValue, kri, rng),
                "StepFunction" => StepFunction(kri, cursor, rng),
                _              => Sinusoidal(kri, cursor, rng),
            };

            var value = Math.Round(Math.Clamp(raw, 0, 120), 2);
            var riskLevel = value <= kri.GreenMax  ? "Green"
                          : value <= kri.YellowMax ? "Yellow"
                          : "Red";

            // Normalize to 0–100 score following the same 3-band formula as the KRI engine
            var normalized = value <= kri.GreenMax
                ? value / kri.GreenMax * 33.0
                : value <= kri.YellowMax
                    ? 33.0 + (value - kri.GreenMax) / (kri.YellowMax - kri.GreenMax) * 33.0
                    : Math.Min(66.0 + (value - kri.YellowMax) / (120.0 - kri.YellowMax) * 34.0, 100.0);

            readings.Add(new KriReading
            {
                Id              = Guid.NewGuid(),
                KriDefinitionId = kri.Id,
                Value           = value,
                NormalizedScore = Math.Round(normalized, 2),
                RiskLevel       = riskLevel,
                Timestamp       = cursor,
                IsSimulated     = true,
            });

            cursor = cursor.AddHours(1);
        }

        return readings;
    }

    // ── Pattern implementations ───────────────────────────────────────────────

    /// <summary>
    /// Peaks around 06:00 and 18:00 UTC (shift-change hours at Klaipėda port).
    /// </summary>
    private static double Sinusoidal(KriDefinition kri, DateTime t, Random rng)
    {
        var angle = 2 * Math.PI * t.Hour / 24.0;
        var dailyCycle = Math.Sin(angle) * (kri.MockVariance * 0.55);
        var noise = (rng.NextDouble() - 0.5) * kri.MockVariance * 0.45;
        return kri.MockBaseline + dailyCycle + noise;
    }

    /// <summary>
    /// Drifts randomly but is pulled back toward the baseline (mean-reverting).
    /// </summary>
    private static double RandomWalk(ref double current, KriDefinition kri, Random rng)
    {
        var step = (rng.NextDouble() - 0.5) * kri.MockVariance * 0.25;
        current += step;
        current += (kri.MockBaseline - current) * 0.03; // reversion force
        return current;
    }

    /// <summary>
    /// Holds a baseline value, then jumps up on inspection days (Mon/Thu) and
    /// Friday-evening weekend-backlog periods — mirroring CustomsDwellTimeService logic.
    /// </summary>
    private static double StepFunction(KriDefinition kri, DateTime t, Random rng)
    {
        var isInspection = t.DayOfWeek is DayOfWeek.Monday or DayOfWeek.Thursday
                           && t.Hour is >= 8 and <= 18;
        var isWeekendBacklog = t.DayOfWeek == DayOfWeek.Friday && t.Hour >= 16;

        var level = isInspection    ? kri.MockBaseline + kri.MockVariance * 0.7
                  : isWeekendBacklog ? kri.MockBaseline + kri.MockVariance * 0.9
                  : kri.MockBaseline;

        var noise = (rng.NextDouble() - 0.5) * kri.MockVariance * 0.15;
        return level + noise;
    }
}
