using Segmentation.Domain.Entities;

namespace Segmentation.Domain.Scoring;

public readonly record struct FarmerSimulationScores(
    int Loyalty,
    int Quality,
    int Financial,
    int Technologies,
    int Esg,
    int Yield,
    int Scale,
    int YieldAndScale)
{
    public int Total => Loyalty + Quality + Financial + Technologies + Esg + Yield + Scale + YieldAndScale;
}

/// <summary>
/// Scores farmers from KPI history, simulation scope seasons, and culture-type configuration rules.
/// See <c>docs/SIMULATION_SCORING.md</c>.
/// </summary>
public static class SimulationFarmerScoring
{
    public static FarmerSimulationScores ComputeScores(
        CultureTypeScoringBundle bundle,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        return new FarmerSimulationScores(
            ScoreLoyalty(bundle.Loyalty, context, history),
            ScoreQuality(bundle.Quality, context, history),
            ScoreFinancial(bundle.Financial, context, history),
            ScoreTechnology(bundle.Technology, context, history),
            ScoreEsg(bundle.Esg, context, history),
            ScoreYield(bundle.Yield, context, history),
            ScoreScale(bundle.Scale, context, history),
            ScoreYieldAndScale(bundle.YieldAndScale, context, history));
    }

    public static SegmentationSegment? PickSegment(
        IEnumerable<SegmentThreshold> thresholds,
        IEnumerable<SegmentationSegment> headerSegments,
        int totalScore,
        bool nonExclusiveFarmer)
    {
        var headerById = headerSegments.ToDictionary(s => s.Id);

        foreach (var t in thresholds
                     .Where(t => t.RangeMin.HasValue)
                     .OrderByDescending(t => t.RangeMin!.Value))
        {
            if (t.OnlyExclusiveFarmer && nonExclusiveFarmer)
                continue;
            if (totalScore >= t.RangeMin!.Value && headerById.TryGetValue(t.SegmentId, out var seg))
                return seg;
        }

        foreach (var t in thresholds.Where(t => !t.RangeMin.HasValue))
        {
            if (t.OnlyExclusiveFarmer && nonExclusiveFarmer)
                continue;
            if (headerById.TryGetValue(t.SegmentId, out var seg))
                return seg;
        }

        return headerSegments.FirstOrDefault(s => !(s.OnlyExclusiveFarmer && nonExclusiveFarmer));
    }

    private static int ScoreLoyalty(
        SegmentationConfigurationLoyalty loyalty,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var selected = context.ScopeCropSeasonIdsDescending;
        if (selected.Count == 0)
            return 0;

        if (loyalty.SeasonQuantityRanges.Count == 0)
        {
            var bestHist = int.MinValue;
            var anyHist = false;
            foreach (var season in selected)
            {
                if (!history.LoyaltyDeliveredPctBySeason.TryGetValue(season, out var delivered))
                    continue;
                foreach (var h in loyalty.HistoricalVolumeRanges)
                {
                    if (delivered < h.MinimumDeliveryAmount || delivered > h.MaximumDeliveryAmount)
                        continue;
                    anyHist = true;
                    bestHist = Math.Max(bestHist, h.Score);
                }
            }

            return anyHist ? bestHist : 0;
        }

        var matching = new List<LoyaltySeasonQuantityRange>();
        foreach (var r in loyalty.SeasonQuantityRanges)
        {
            var plantingSeasonsWithData = selected.Count(season =>
                history.LoyaltyDeliveredPctBySeason.ContainsKey(season));

            if (plantingSeasonsWithData < r.PlantingCropSeasonAmount)
                continue;

            if (!DeliveryWindowSatisfied(
                    selected,
                    r.DeliveryCropSeasonAmount,
                    season =>
                    {
                        if (!history.LoyaltyDeliveredPctBySeason.TryGetValue(season, out var v))
                            return false;
                        return v >= r.MinimumDeliveryAmount && v <= r.MaximumDeliveryAmount;
                    }))
                continue;

            matching.Add(r);
        }

        if (matching.Count == 0)
            return 0;

        return matching.OrderByDescending(r => r.PlantingCropSeasonAmount).First().Score;
    }

    private static int ScoreQuality(
        SegmentationConfigurationQuality quality,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var season = context.LatestScopeCropSeasonId;
        var score = 0;

        if (history.QualityBySeason.TryGetValue(season, out var current))
        {
            foreach (var r in quality.IqsRanges)
            {
                if (current.Iqs >= r.Minimum && current.Iqs <= r.Maximum)
                {
                    score = r.Score;
                    break;
                }
            }

            if (current.HadNtrm)
                score += quality.NtrmScore;
            if (current.HadQualityMixture)
                score += quality.MixtureScore;
        }

        return score;
    }

    private static int ScoreFinancial(
        SegmentationConfigurationFinancial financial,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var season = context.LatestScopeCropSeasonId;
        if (!history.FinancialBySeason.TryGetValue(season, out var cur))
            return 0;

        var score = 0;
        foreach (var r in financial.SelfFundingRanges)
        {
            if (cur.SelfFundingPercentage >= r.Minimum && cur.SelfFundingPercentage <= r.Maximum)
            {
                score = r.Score;
                break;
            }
        }

        if (cur.HaveDebt)
            score += financial.DebtScore;

        return score;
    }

    private static int ScoreTechnology(
        SegmentationConfigurationTechnology technology,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var sum = 0;
        var season = context.LatestScopeCropSeasonId;

        if (!history.TechnologiesBySeason.TryGetValue(season, out var t))
            return 0;

        if (t.HasLargeBaseRidgeWithMulch)
            sum += technology.HasLargeBaseRidgeWithMulchScore;
        if (t.HasBroadGrateFurnace)
            sum += technology.HasBroadGrateFurnaceScore;
        if (t.HasTechnologyPackageAdherence)
            sum += technology.HasTechnologyPackageAdherenceScore;
        if (t.HasStandardBarn)
            sum += technology.HasStandardBarnScore;

        return sum;
    }

    private static int ScoreEsg(
        SegmentationConfigurationEsg esg,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var score = 0;
        var season = context.LatestScopeCropSeasonId;

        if (history.EsgBySeason.TryGetValue(season, out var kr))
        {
            var add = kr.ReforestationPercentage * esg.ReforestationScorePerPercentualPoint;
            score += Math.Clamp(add, 0, esg.ReforestationMaximumScore);
        }

        if (history.EsgBySeason.TryGetValue(season, out var kn))
        {
            var add = kn.NativeForestPercentage * esg.NativeForestScorePerPercentualPoint;
            score += Math.Clamp(add, 0, esg.NativeForestMaximumScore);
        }

        if (history.EsgBySeason.TryGetValue(season, out var km) && km.HasMinorIrregularity)
            score += esg.MinorIrregularityScore;
        if (history.EsgBySeason.TryGetValue(season, out var kM) && kM.HasMajorIrregularity)
            score += esg.MajorIrregularityScore;

        return score;
    }

    private static int ScoreYield(
        SegmentationConfigurationYield yield,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var season = context.LatestScopeCropSeasonId;
        if (!history.YieldBySeason.TryGetValue(season, out var y))
            return 0;

        foreach (var r in yield.Ranges)
        {
            if (y >= r.Minimum && y <= r.Maximum)
                return r.Score;
        }

        return 0;
    }

    private static int ScoreScale(
        SegmentationConfigurationScale scale,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var season = context.LatestScopeCropSeasonId;
        if (!history.ScaleBySeason.TryGetValue(season, out var s))
            return 0;

        foreach (var r in scale.Ranges)
        {
            if (s >= r.Minimum && s <= r.Maximum)
                return r.Score;
        }

        return 0;
    }

    private static int ScoreYieldAndScale(
        SegmentationConfigurationYieldAndScale yieldAndScale,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var selected = context.ScopeCropSeasonIdsDescending;
        if (selected.Count == 0 || yieldAndScale.Ranges.Count == 0)
            return 0;

        var seasonsWithYieldAndScale = selected
            .Where(s => history.YieldBySeason.ContainsKey(s) && history.ScaleBySeason.ContainsKey(s))
            .ToList();

        var plantingSeasonsWithData = seasonsWithYieldAndScale.Count;
        if (plantingSeasonsWithData == 0)
            return 0;

        var matching = new List<YieldAndScaleRange>();
        foreach (var r in yieldAndScale.Ranges)
        {
            if (plantingSeasonsWithData < r.YieldAndScaleCropSeasonAmount)
                continue;

            var avgYield = (int)Math.Round(seasonsWithYieldAndScale.Average(s => history.YieldBySeason[s]));
            var avgModule = Math.Round(
                seasonsWithYieldAndScale.Average(s => (decimal)history.ScaleBySeason[s]),
                1,
                MidpointRounding.AwayFromZero);

            if (avgYield < r.MinimumYield || avgYield > r.MaximumYield)
                continue;
            if (avgModule < r.MinimumModule || avgModule > r.MaximumModule)
                continue;

            matching.Add(r);
        }

        if (matching.Count == 0)
            return 0;

        return matching.OrderByDescending(r => r.YieldAndScaleCropSeasonAmount).First().Score;
    }

    /// <summary>
    /// The <paramref name="cropSeasonAmount"/> most recent seasons in <paramref name="selectedSeasonsDescending"/>
    /// must all satisfy <paramref name="predicate"/> (loyalty only).
    /// </summary>
    private static bool DeliveryWindowSatisfied(
        IReadOnlyList<int> selectedSeasonsDescending,
        int cropSeasonAmount,
        Func<int, bool> predicate)
    {
        if (cropSeasonAmount <= 0 || selectedSeasonsDescending.Count < cropSeasonAmount)
            return false;

        foreach (var season in selectedSeasonsDescending.Take(cropSeasonAmount))
        {
            if (!predicate(season))
                return false;
        }

        return true;
    }
}
