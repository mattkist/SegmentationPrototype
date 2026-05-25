using Segmentation.Domain.Entities;

namespace Segmentation.Domain;

/// <summary>
/// Derives per-KPI MaxScore values from configuration rules and validates the configured total per culture type.
/// </summary>
public static class SegmentationConfigurationKpiMaxScores
{
    public static DerivedKpiMaxScores ComputeDerivedMaxScores(SegmentationConfigurationCultureType cultureType)
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
        var loyaltyDerived = MaxPositiveOrZero(loyalty.SeasonQuantityRanges.Select(r => r.Score))
            + MaxPositiveOrZero(loyalty.HistoricalVolumeRanges.Select(r => r.Score));

        var quality = cultureType.Quality;
        var qualityDerived = quality.IqsRanges.Count == 0
            ? 0
            : quality.IqsRanges.Max(r => r.Score);

        var financial = cultureType.Financial;
        var financialDerived = financial.SelfFundingRanges.Count == 0
            ? 0
            : financial.SelfFundingRanges.Max(r => r.Score);

        var technology = cultureType.Technology;
        var technologyDerived = SumPositive(technology.TechnologyScores.Select(s => s.Score));

        var esg = cultureType.Esg;
        var esgDerived = esg.ReforestationMaximumScore
            + esg.NativeForestMaximumScore
            + SumPositive(esg.IrregularityScores.Select(s => s.Score));

        var yield = cultureType.Yield;
        var yieldDerived = yield.Ranges.Count == 0 ? 0 : yield.Ranges.Max(r => r.Score);

        var scale = cultureType.Scale;
        var scaleDerived = scale.Ranges.Count == 0 ? 0 : scale.Ranges.Max(r => r.Score);

        var yieldAndScale = cultureType.YieldAndScale;
        var yieldAndScaleDerived = MaxPositiveOrZero(yieldAndScale.Ranges.Select(r => r.Score));

        return new DerivedKpiMaxScores(
            loyaltyDerived,
            qualityDerived,
            financialDerived,
            technologyDerived,
            esgDerived,
            yieldDerived,
            scaleDerived,
            yieldAndScaleDerived);
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
        var errors = new List<string>();

        DerivedKpiMaxScores derived;
        try
        {
            derived = ComputeDerivedMaxScores(cultureType);
        }
        catch (InvalidOperationException ex)
        {
            return new KpiMaxScoreValidationResult(false, 0, [ex.Message]);
        }

        var sum = SumKpiMaxScores(cultureType);
        if (sum != cultureType.MaximumScore)
        {
            errors.Add(
                $"Sum of KPI max scores ({sum}) must equal MaximumScore ({cultureType.MaximumScore}) for culture type '{cultureType.CultureTypeCode}'.");
        }

        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Loyalty", cultureType.Loyalty!.MaxScore, derived.Loyalty);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Quality", cultureType.Quality!.MaxScore, derived.Quality);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Financial", cultureType.Financial!.MaxScore, derived.Financial);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Technology", cultureType.Technology!.MaxScore, derived.Technology);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "ESG", cultureType.Esg!.MaxScore, derived.Esg);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Yield", cultureType.Yield!.MaxScore, derived.Yield);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Scale", cultureType.Scale!.MaxScore, derived.Scale);
        ValidateBlockMaxScore(errors, cultureType.CultureTypeCode, "Yield & Scale", cultureType.YieldAndScale!.MaxScore, derived.YieldAndScale);

        return new KpiMaxScoreValidationResult(
            errors.Count == 0,
            sum,
            errors.Count > 0 ? errors : null);
    }

    private static void ValidateBlockMaxScore(
        List<string> errors,
        string cultureTypeCode,
        string blockName,
        int configuredMaxScore,
        int derivedMaxScore)
    {
        if (configuredMaxScore != derivedMaxScore)
        {
            errors.Add(
                $"{blockName} ({cultureTypeCode}): configured MaxScore is {configuredMaxScore}, but derived max from rules is {derivedMaxScore}.");
        }
    }

    private static int MaxPositiveOrZero(IEnumerable<int> scores) =>
        scores.Where(s => s > 0).DefaultIfEmpty(0).Max();

    private static int SumPositive(IEnumerable<int> scores) =>
        scores.Where(s => s > 0).Sum();
}

public readonly record struct DerivedKpiMaxScores(
    int Loyalty,
    int Quality,
    int Financial,
    int Technology,
    int Esg,
    int Yield,
    int Scale,
    int YieldAndScale);

public readonly record struct KpiMaxScoreValidationResult(
    bool IsValid,
    int SumOfKpiMaxScores,
    IReadOnlyList<string>? Errors)
{
    public string? ErrorMessage => Errors is { Count: > 0 } ? string.Join(" ", Errors) : null;
}
