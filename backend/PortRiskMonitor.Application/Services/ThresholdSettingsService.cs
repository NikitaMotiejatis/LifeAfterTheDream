using PortRiskMonitor.Application.DTOs;
using PortRiskMonitor.Application.Interfaces;
using RiskMonitor.Repositories;

namespace PortRiskMonitor.Application.Services;

public class ThresholdSettingsService : IThresholdSettingsService
{
    private readonly IKriAdminRepository _kriRepo;

    // Seed defaults — kept in sync with SeedData.cs. Used by ResetAsync.
    private static readonly Dictionary<string, ThresholdPairDto> SeedDefaults = new()
    {
        ["port-status"]   = new(30, 60),
        ["berth"]         = new(70, 90),
        ["vessel-delays"] = new(10, 25),
        ["weather"]       = new(20, 40),
        ["customs"]       = new(24, 72),
    };

    public ThresholdSettingsService(IKriAdminRepository kriRepo)
    {
        _kriRepo = kriRepo;
    }

    public async Task<ThresholdSettingsDto> GetAllAsync()
    {
        var kris = await _kriRepo.GetAllAsync();
        var result = new ThresholdSettingsDto();
        foreach (var kri in kris)
        {
            result[kri.Slug] = new ThresholdPairDto(kri.GreenMax, kri.YellowMax);
        }
        return result;
    }

    public async Task<ThresholdSettingsDto> UpdateAsync(ThresholdSettingsDto settings)
    {
        foreach (var (slug, pair) in settings)
        {
            ValidatePair(slug, pair);

            var kri = await _kriRepo.GetBySlugAsync(slug)
                ?? throw new InvalidOperationException($"Unknown metric slug: {slug}");

            // GetBySlugAsync returns AsNoTracking — re-attach for update.
            kri.GreenMax = pair.Green;
            kri.YellowMax = pair.Yellow;
            await _kriRepo.UpdateAsync(kri);
        }
        return await GetAllAsync();
    }

    public async Task<ThresholdSettingsDto> ResetAsync()
    {
        var defaults = new ThresholdSettingsDto(SeedDefaults);
        return await UpdateAsync(defaults);
    }

    private static void ValidatePair(string slug, ThresholdPairDto pair)
    {
        if (double.IsNaN(pair.Green) || double.IsNaN(pair.Yellow))
            throw new ArgumentException($"{slug}: thresholds must be numeric.");
        if (pair.Green < 0 || pair.Yellow < 0)
            throw new ArgumentException($"{slug}: thresholds must be non-negative.");
        if (pair.Green >= pair.Yellow)
            throw new ArgumentException($"{slug}: green ({pair.Green}) must be less than yellow ({pair.Yellow}).");
    }
}
