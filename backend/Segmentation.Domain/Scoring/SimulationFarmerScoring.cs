using Segmentation.Domain.Entities;

namespace Segmentation.Domain.Scoring;

public readonly record struct FarmerSimulationScores(
    int Loyalty,
    int Quality,
    int Financial,
    int Technologies,
    int Esg,
    int Yield,
    int Scale)
{
    public int Total => Loyalty + Quality + Financial + Technologies + Esg + Yield + Scale;
}

/// <summary>
/// Scores farmers from KPI history, per-KPI simulation scopes, and culture-type configuration rules.
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
            ScoreLoyalty(bundle.Loyalty, context.KpiScopes.Loyalty, history),
            ScoreQuality(bundle.Quality, context.KpiScopes.Quality, history),
            ScoreFinancial(bundle.Financial, context.KpiScopes.Financial, history),
            ScoreTechnology(bundle.Technology, context.KpiScopes.Technologies, history),
            ScoreEsg(bundle.Esg, context.KpiScopes.Esg, history),
            ScoreYield(bundle.Yield, context.KpiScopes.Yield, history),
            ScoreScale(bundle.Scale, context.KpiScopes.Scale, history));
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
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var selected = scope.CropSeasonIdsDescending;
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
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var score = 0;
        var iqsValue = ResolveNumericFromScope(
            scope,
            season => history.QualityBySeason.TryGetValue(season, out var q) ? q.Iqs : null);

        if (iqsValue.HasValue)
        {
            foreach (var r in quality.IqsRanges)
            {
                if (iqsValue.Value >= r.Minimum && iqsValue.Value <= r.Maximum)
                {
                    score = r.Score;
                    break;
                }
            }
        }

        var latestSeason = LatestSeasonWithData(scope.CropSeasonIdsDescending, history.QualityBySeason.Keys);
        if (latestSeason is not null && history.QualityBySeason.TryGetValue(latestSeason.Value, out var current))
        {
            if (current.HadNtrm)
                score += quality.NtrmScore;
            if (current.HadQualityMixture)
                score += quality.MixtureScore;
        }

        return score;
    }

    private static int ScoreFinancial(
        SegmentationConfigurationFinancial financial,
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var score = 0;
        var selfFunding = ResolveNumericFromScope(
            scope,
            season => history.FinancialBySeason.TryGetValue(season, out var f) ? f.SelfFundingPercentage : null);

        if (selfFunding.HasValue)
        {
            foreach (var r in financial.SelfFundingRanges)
            {
                if (selfFunding.Value >= r.Minimum && selfFunding.Value <= r.Maximum)
                {
                    score = r.Score;
                    break;
                }
            }
        }

        var latestSeason = LatestSeasonWithData(scope.CropSeasonIdsDescending, history.FinancialBySeason.Keys);
        if (latestSeason is not null
            && history.FinancialBySeason.TryGetValue(latestSeason.Value, out var cur)
            && cur.HaveDebt)
        {
            score += financial.DebtScore;
        }

        return score;
    }

    private static int ScoreTechnology(
        SegmentationConfigurationTechnology technology,
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var latestSeason = LatestSeasonWithData(scope.CropSeasonIdsDescending, history.TechnologiesBySeason.Keys);
        if (latestSeason is null || !history.TechnologiesBySeason.TryGetValue(latestSeason.Value, out var t))
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
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var score = 0;
        var latestSeason = LatestSeasonWithData(scope.CropSeasonIdsDescending, history.EsgBySeason.Keys);
        if (latestSeason is null || !history.EsgBySeason.TryGetValue(latestSeason.Value, out var kr))
            return 0;

        var add = kr.ReforestationPercentage * esg.ReforestationScorePerPercentualPoint;
        score += Math.Clamp(add, 0, esg.ReforestationMaximumScore);

        add = kr.NativeForestPercentage * esg.NativeForestScorePerPercentualPoint;
        score += Math.Clamp(add, 0, esg.NativeForestMaximumScore);

        foreach (var rule in esg.IrregularityScores)
        {
            if (rule.Score > 0 && kr.IrregularityTypeIds.Contains(rule.IrregularityTypeId))
                score += rule.Score;
        }

        return score;
    }

    private static int ScoreYield(
        SegmentationConfigurationYield yield,
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var yieldValue = ResolveNumericFromScope(
            scope,
            season => history.ContractBySeason.TryGetValue(season, out var c) ? c.Yield : null);

        if (!yieldValue.HasValue)
            return 0;

        foreach (var r in yield.Ranges)
        {
            if (yieldValue.Value >= r.Minimum && yieldValue.Value <= r.Maximum)
                return r.Score;
        }

        return 0;
    }

    private static int ScoreScale(
        SegmentationConfigurationScale scale,
        KpiScope scope,
        FarmerKpiHistory history)
    {
        var scaleValue = ResolveNumericFromScope(
            scope,
            season => history.ContractBySeason.TryGetValue(season, out var c) ? c.Scale : null);

        if (!scaleValue.HasValue)
            return 0;

        var rounded = (int)Math.Round((decimal)scaleValue.Value, MidpointRounding.AwayFromZero);
        foreach (var r in scale.Ranges)
        {
            if (rounded >= r.Minimum && rounded <= r.Maximum)
                return r.Score;
        }

        return 0;
    }

    private static int? ResolveNumericFromScope(
        KpiScope scope,
        Func<int, int?> getValueForSeason)
    {
        var seasonsWithData = scope.CropSeasonIdsDescending
            .Select(s => (Season: s, Value: getValueForSeason(s)))
            .Where(x => x.Value.HasValue)
            .ToList();

        if (seasonsWithData.Count == 0)
            return null;

        if (scope.ValueAggregation == KpiValueAggregation.Average)
        {
            return (int)Math.Round(
                seasonsWithData.Average(x => x.Value!.Value),
                MidpointRounding.AwayFromZero);
        }

        return seasonsWithData[0].Value;
    }

    private static int? LatestSeasonWithData(
        IReadOnlyList<int> seasonsDescending,
        IEnumerable<int> seasonsWithData)
    {
        var set = seasonsWithData.ToHashSet();
        foreach (var season in seasonsDescending)
        {
            if (set.Contains(season))
                return season;
        }

        return null;
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
