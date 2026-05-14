using Segmentation.Domain.Entities;

namespace Segmentation.Domain;

/// <summary>
/// Derives per-KPI MaxScore values from configuration rules and validates the configured total.
/// </summary>
public static class SegmentationConfigurationKpiMaxScores
{
    public static void SynchronizeFromRules(SegmentationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration.Loyalty is null)
            throw new InvalidOperationException("Loyalty configuration is required.");
        if (configuration.Quality is null)
            throw new InvalidOperationException("Quality configuration is required.");
        if (configuration.Financial is null)
            throw new InvalidOperationException("Financial configuration is required.");
        if (configuration.Technology is null)
            throw new InvalidOperationException("Technology configuration is required.");
        if (configuration.Esg is null)
            throw new InvalidOperationException("ESG configuration is required.");
        if (configuration.Yield is null)
            throw new InvalidOperationException("Yield configuration is required.");
        if (configuration.Scale is null)
            throw new InvalidOperationException("Scale configuration is required.");

        var loyalty = configuration.Loyalty;
        loyalty.MaxScore = MaxPositiveOrZero(loyalty.SeasonQuantityRanges.Select(r => r.Score))
            + MaxPositiveOrZero(loyalty.HistoricalVolumeRanges.Select(r => r.Score));

        var quality = configuration.Quality;
        quality.MaxScore = quality.IqsRanges.Count == 0
            ? 0
            : quality.IqsRanges.Max(r => r.Score);

        var financial = configuration.Financial;
        financial.MaxScore = financial.SelfFundingRanges.Count == 0
            ? 0
            : financial.SelfFundingRanges.Max(r => r.Score);

        var technology = configuration.Technology;
        technology.MaxScore = SumPositive(
            technology.HasLargeBaseRidgeWithMulchScore,
            technology.HasBroadGrateFurnaceScore,
            technology.HasTechnologyPackageAdherenceScore);

        var esg = configuration.Esg;
        esg.MaxScore = esg.ReforestationMaximumScore + esg.NativeForestMaximumScore;

        var yield = configuration.Yield;
        yield.MaxScore = yield.Ranges.Count == 0 ? 0 : yield.Ranges.Max(r => r.Score);

        var scale = configuration.Scale;
        scale.MaxScore = scale.Ranges.Count == 0 ? 0 : scale.Ranges.Max(r => r.Score);
    }

    public static int SumKpiMaxScores(SegmentationConfiguration configuration)
    {
        return configuration.Loyalty!.MaxScore
            + configuration.Quality!.MaxScore
            + configuration.Financial!.MaxScore
            + configuration.Technology!.MaxScore
            + configuration.Esg!.MaxScore
            + configuration.Yield!.MaxScore
            + configuration.Scale!.MaxScore;
    }

    public static KpiMaxScoreValidationResult ValidateAgainstMaximum(SegmentationConfiguration configuration)
    {
        try
        {
            SynchronizeFromRules(configuration);
        }
        catch (InvalidOperationException ex)
        {
            return new KpiMaxScoreValidationResult(false, 0, ex.Message);
        }

        var sum = SumKpiMaxScores(configuration);
        if (sum != configuration.MaximumScore)
        {
            return new KpiMaxScoreValidationResult(
                false,
                sum,
                $"Sum of KPI max scores ({sum}) must equal MaximumScore ({configuration.MaximumScore}).");
        }

        return new KpiMaxScoreValidationResult(true, sum, null);
    }

    private static int MaxPositiveOrZero(IEnumerable<int> scores) =>
        scores.Where(s => s > 0).DefaultIfEmpty(0).Max();

    private static int SumPositive(params int[] scores) =>
        scores.Where(s => s > 0).Sum();
}

public readonly record struct KpiMaxScoreValidationResult(bool IsValid, int SumOfKpiMaxScores, string? ErrorMessage);
