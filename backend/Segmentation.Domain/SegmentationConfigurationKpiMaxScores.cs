using Segmentation.Domain.Entities;

namespace Segmentation.Domain;

/// <summary>
/// Derives per-KPI MaxScore values from configuration rules and validates the configured total per culture type.
/// </summary>
public static class SegmentationConfigurationKpiMaxScores
{
    public static void SynchronizeFromRules(SegmentationConfigurationCultureType cultureType)
    {
        ArgumentNullException.ThrowIfNull(cultureType);

        if (cultureType.Loyalty is null)
            throw new InvalidOperationException("Loyalty configuration is required.");
        if (cultureType.Quality is null)
            throw new InvalidOperationException("Quality configuration is required.");
        if (cultureType.Financial is null)
            throw new InvalidOperationException("Financial configuration is required.");
        if (cultureType.Technology is null)
            throw new InvalidOperationException("Technology configuration is required.");
        if (cultureType.Esg is null)
            throw new InvalidOperationException("ESG configuration is required.");
        if (cultureType.Yield is null)
            throw new InvalidOperationException("Yield configuration is required.");
        if (cultureType.Scale is null)
            throw new InvalidOperationException("Scale configuration is required.");
        if (cultureType.YieldAndScale is null)
            throw new InvalidOperationException("Yield & Scale configuration is required.");

        var loyalty = cultureType.Loyalty;
        loyalty.MaxScore = MaxPositiveOrZero(loyalty.SeasonQuantityRanges.Select(r => r.Score))
            + MaxPositiveOrZero(loyalty.HistoricalVolumeRanges.Select(r => r.Score));

        var quality = cultureType.Quality;
        quality.MaxScore = quality.IqsRanges.Count == 0
            ? 0
            : quality.IqsRanges.Max(r => r.Score);

        var financial = cultureType.Financial;
        financial.MaxScore = financial.SelfFundingRanges.Count == 0
            ? 0
            : financial.SelfFundingRanges.Max(r => r.Score);

        var technology = cultureType.Technology;
        technology.MaxScore = SumPositive(
            technology.HasLargeBaseRidgeWithMulchScore,
            technology.HasBroadGrateFurnaceScore,
            technology.HasTechnologyPackageAdherenceScore,
            technology.HasStandardBarnScore);

        var esg = cultureType.Esg;
        esg.MaxScore = esg.ReforestationMaximumScore + esg.NativeForestMaximumScore;

        var yield = cultureType.Yield;
        yield.MaxScore = yield.Ranges.Count == 0 ? 0 : yield.Ranges.Max(r => r.Score);

        var scale = cultureType.Scale;
        scale.MaxScore = scale.Ranges.Count == 0 ? 0 : scale.Ranges.Max(r => r.Score);

        var yieldAndScale = cultureType.YieldAndScale;
        yieldAndScale.MaxScore = MaxPositiveOrZero(yieldAndScale.Ranges.Select(r => r.Score));
    }

    public static void SynchronizeAll(SegmentationConfiguration configuration)
    {
        foreach (var ct in configuration.CultureTypes)
            SynchronizeFromRules(ct);
    }

    public static int SumKpiMaxScores(SegmentationConfigurationCultureType cultureType)
    {
        return cultureType.Loyalty!.MaxScore
            + cultureType.Quality!.MaxScore
            + cultureType.Financial!.MaxScore
            + cultureType.Technology!.MaxScore
            + cultureType.Esg!.MaxScore
            + cultureType.Yield!.MaxScore
            + cultureType.Scale!.MaxScore
            + cultureType.YieldAndScale!.MaxScore;
    }

    public static KpiMaxScoreValidationResult ValidateCultureType(SegmentationConfigurationCultureType cultureType)
    {
        try
        {
            SynchronizeFromRules(cultureType);
        }
        catch (InvalidOperationException ex)
        {
            return new KpiMaxScoreValidationResult(false, 0, ex.Message);
        }

        var sum = SumKpiMaxScores(cultureType);
        if (sum != cultureType.MaximumScore)
        {
            return new KpiMaxScoreValidationResult(
                false,
                sum,
                $"Sum of KPI max scores ({sum}) must equal MaximumScore ({cultureType.MaximumScore}) for culture type '{cultureType.CultureTypeCode}'.");
        }

        return new KpiMaxScoreValidationResult(true, sum, null);
    }

    private static int MaxPositiveOrZero(IEnumerable<int> scores) =>
        scores.Where(s => s > 0).DefaultIfEmpty(0).Max();

    private static int SumPositive(params int[] scores) =>
        scores.Where(s => s > 0).Sum();
}

public readonly record struct KpiMaxScoreValidationResult(bool IsValid, int SumOfKpiMaxScores, string? ErrorMessage);
