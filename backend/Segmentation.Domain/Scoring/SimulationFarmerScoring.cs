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
            var consolidatedDelivered = ConsolidatedDeliveryPercentage(selected, history);
            var bestHist = int.MinValue;
            var anyHist = false;
            foreach (var h in loyalty.HistoricalVolumeRanges)
            {
                if (consolidatedDelivered < h.MinimumDeliveryAmount || consolidatedDelivered > h.MaximumDeliveryAmount)
                    continue;
                anyHist = true;
                bestHist = Math.Max(bestHist, h.Score);
            }

            return anyHist ? bestHist : 0;
        }

        var matching = new List<LoyaltySeasonQuantityRange>();
        foreach (var r in loyalty.SeasonQuantityRanges)
        {
            var plantingSeasonsWithData = selected.Count(season =>
                history.LoyaltyBySeason.ContainsKey(season));

            if (plantingSeasonsWithData < r.PlantingCropSeasonAmount)
                continue;

            if (!DeliveryWindowSatisfied(
                    selected,
                    r.DeliveryCropSeasonAmount,
                    season =>
                    {
                        if (!history.LoyaltyBySeason.TryGetValue(season, out var snapshot))
                            return false;
                        var delivered = DeliveryPercentage(snapshot.DeliveredAmountKg, snapshot.ContractedAmountKg);
                        return delivered >= r.MinimumDeliveryAmount && delivered <= r.MaximumDeliveryAmount;
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
        var season = context.LatestScopeCropSeasonId;

        if (!history.TechnologiesBySeason.TryGetValue(season, out var t))
            return 0;

        var sum = 0;
        foreach (var rule in technology.TechnologyScores)
        {
            if (rule.Score > 0 && t.TechnologyIds.Contains(rule.TechnologyId))
                sum += rule.Score;
        }

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

            add = kr.NativeForestPercentage * esg.NativeForestScorePerPercentualPoint;
            score += Math.Clamp(add, 0, esg.NativeForestMaximumScore);

            foreach (var rule in esg.IrregularityScores)
            {
                if (rule.Score > 0 && kr.IrregularityTypeIds.Contains(rule.IrregularityTypeId))
                    score += rule.Score;
            }
        }

        return score;
    }

    private static int ScoreYield(
        SegmentationConfigurationYield yield,
        SimulationScoringContext context,
        FarmerKpiHistory history)
    {
        var season = context.LatestScopeCropSeasonId;
        if (!history.YieldAndScaleBySeason.TryGetValue(season, out var snapshot))
            return 0;

        foreach (var r in yield.Ranges)
        {
            if (snapshot.Yield >= r.Minimum && snapshot.Yield <= r.Maximum)
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
        if (!history.YieldAndScaleBySeason.TryGetValue(season, out var snapshot))
            return 0;

        foreach (var r in scale.Ranges)
        {
            if (snapshot.Scale >= r.Minimum && snapshot.Scale <= r.Maximum)
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
            .Where(s => history.YieldAndScaleBySeason.ContainsKey(s))
            .ToList();

        var plantingSeasonsWithData = seasonsWithYieldAndScale.Count;
        if (plantingSeasonsWithData == 0)
            return 0;

        var totalContracted = seasonsWithYieldAndScale.Sum(s => history.YieldAndScaleBySeason[s].ContractedAmountKg);
        var totalScale = seasonsWithYieldAndScale.Sum(s => history.YieldAndScaleBySeason[s].Scale);
        if (totalScale == 0)
            return 0;

        var consolidatedYield = totalContracted / totalScale;
        var avgModule = Math.Round(
            seasonsWithYieldAndScale.Average(s => (decimal)history.YieldAndScaleBySeason[s].Scale),
            1,
            MidpointRounding.AwayFromZero);

        var matching = new List<YieldAndScaleRange>();
        foreach (var r in yieldAndScale.Ranges)
        {
            if (plantingSeasonsWithData < r.YieldAndScaleCropSeasonAmount)
                continue;

            if (consolidatedYield < r.MinimumYield || consolidatedYield > r.MaximumYield)
                continue;
            if (avgModule < r.MinimumModule || avgModule > r.MaximumModule)
                continue;

            matching.Add(r);
        }

        if (matching.Count == 0)
            return 0;

        return matching.OrderByDescending(r => r.YieldAndScaleCropSeasonAmount).First().Score;
    }

    private static int DeliveryPercentage(int deliveredAmountKg, int contractedAmountKg) =>
        contractedAmountKg > 0
            ? (int)Math.Round(deliveredAmountKg * 100m / contractedAmountKg, MidpointRounding.AwayFromZero)
            : 0;

    private static int ConsolidatedDeliveryPercentage(
        IReadOnlyList<int> seasons,
        FarmerKpiHistory history)
    {
        var delivered = 0;
        var contracted = 0;
        foreach (var season in seasons)
        {
            if (!history.LoyaltyBySeason.TryGetValue(season, out var snapshot))
                continue;
            delivered += snapshot.DeliveredAmountKg;
            contracted += snapshot.ContractedAmountKg;
        }

        return DeliveryPercentage(delivered, contracted);
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
