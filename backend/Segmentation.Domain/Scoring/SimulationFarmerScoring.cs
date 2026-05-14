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

public static class SimulationFarmerScoring
{
    /// <summary>
    /// Computes KPI component scores for one farmer using only KPI history and rule rows in
    /// <paramref name="configuration"/>. Calendar seasons come from each rule (e.g. <c>CropSeasonStart</c> on ranges,
    /// or the season fields on technology/ESG). The simulation's crop season is <strong>not</strong> used here—it is
    /// only stored with the simulation for reporting and <see cref="FarmerSegmentation"/> by season.
    /// </summary>
    public static FarmerSimulationScores ComputeScores(
        SegmentationConfiguration configuration,
        FarmerKpiHistory history)
    {
        var loyalty = configuration.Loyalty is null ? 0 : ScoreLoyalty(configuration.Loyalty, history);
        var quality = configuration.Quality is null ? 0 : ScoreQuality(configuration.Quality, history);
        var financial = configuration.Financial is null ? 0 : ScoreFinancial(configuration.Financial, history);
        var technology = configuration.Technology is null ? 0 : ScoreTechnology(configuration.Technology, history);
        var esg = configuration.Esg is null ? 0 : ScoreEsg(configuration.Esg, history);
        var yield = configuration.Yield is null ? 0 : ScoreYield(configuration.Yield, history);
        var scale = configuration.Scale is null ? 0 : ScoreScale(configuration.Scale, history);

        return new FarmerSimulationScores(loyalty, quality, financial, technology, esg, yield, scale);
    }

    /// <summary>
    /// Assigns a segment by comparing <paramref name="totalScore"/> to each segment's <c>RangeMin</c> (highest threshold first).
    /// <c>RangeMin</c> may be negative when scoring rules produce negative totals; the check is <c>totalScore &gt;= RangeMin</c>.
    /// </summary>
    public static SegmentationSegment? PickSegment(
        IEnumerable<SegmentationSegment> segments,
        int totalScore,
        bool nonExclusiveFarmer)
    {
        foreach (var seg in segments
                     .Where(s => s.RangeMin.HasValue)
                     .OrderByDescending(s => s.RangeMin!.Value))
        {
            if (seg.OnlyExclusiveFarmer && nonExclusiveFarmer)
                continue;
            if (totalScore >= seg.RangeMin!.Value)
                return seg;
        }

        foreach (var seg in segments.Where(s => !s.RangeMin.HasValue))
        {
            if (seg.OnlyExclusiveFarmer && nonExclusiveFarmer)
                continue;
            return seg;
        }

        return segments.FirstOrDefault(s => !(s.OnlyExclusiveFarmer && nonExclusiveFarmer));
    }

    public static IReadOnlyDictionary<Guid, int> CompetitionRanks(IReadOnlyList<(Guid FarmerId, int TotalScore)> rows)
    {
        var dict = new Dictionary<Guid, int>(rows.Count);
        foreach (var (farmerId, total) in rows)
        {
            var higher = rows.Count(x => x.TotalScore > total);
            dict[farmerId] = higher + 1;
        }

        return dict;
    }

    private static int ScoreLoyalty(SegmentationConfigurationLoyalty loyalty, FarmerKpiHistory history)
    {
        var best = int.MinValue;
        var any = false;

        if (loyalty.SeasonQuantityRanges.Count == 0)
        {
            foreach (var delivered in history.LoyaltyDeliveredPctBySeason.Values)
            {
                foreach (var h in loyalty.HistoricalVolumeRanges)
                {
                    if (delivered < h.MinimumDeliveryAmount || delivered > h.MaximumDeliveryAmount)
                        continue;
                    any = true;
                    best = Math.Max(best, h.Score);
                }
            }
        }
        else
        {
            foreach (var r in loyalty.SeasonQuantityRanges)
            {
                var anchor = r.CropSeasonStart;
                if (history.LoyaltyDeliveredPctBySeason.TryGetValue(anchor, out var deliveredAtAnchor))
                {
                    foreach (var h in loyalty.HistoricalVolumeRanges)
                    {
                        if (deliveredAtAnchor < h.MinimumDeliveryAmount || deliveredAtAnchor > h.MaximumDeliveryAmount)
                            continue;
                        any = true;
                        best = Math.Max(best, h.Score);
                    }
                }

                var skipped = r.SkippedCropSeasons.Select(s => s.CropSeasonId).ToHashSet();
                var plantingFrom = anchor - r.PlantingCropSeasonAmount + 1;
                var plantingSeasonsWithData = 0;
                for (var season = plantingFrom; season <= anchor; season++)
                {
                    if (skipped.Contains(season))
                        continue;
                    if (history.LoyaltyDeliveredPctBySeason.ContainsKey(season))
                        plantingSeasonsWithData++;
                }

                if (plantingSeasonsWithData < r.PlantingCropSeasonAmount)
                    continue;

                if (!WindowValuesSatisfied(
                        anchor,
                        r.DeliveryCropSeasonAmount,
                        skipped,
                        season => history.LoyaltyDeliveredPctBySeason.TryGetValue(season, out var v)
                                  && v >= r.MinimumDeliveryAmount
                                  && v <= r.MaximumDeliveryAmount))
                    continue;

                any = true;
                best = Math.Max(best, r.Score);
            }
        }

        return any ? best : 0;
    }

    private static int ScoreQuality(SegmentationConfigurationQuality quality, FarmerKpiHistory history)
    {
        var best = int.MinValue;
        var any = false;

        foreach (var r in quality.IqsRanges)
        {
            var anchor = r.CropSeasonStart;
            if (!WindowValuesSatisfied(
                    anchor,
                    r.CropSeasonAmount,
                    r.SkippedCropSeasons.Select(s => s.CropSeasonId).ToHashSet(),
                    season =>
                    {
                        if (!history.QualityBySeason.TryGetValue(season, out var q))
                            return false;
                        return q.Iqs >= r.Minimum && q.Iqs <= r.Maximum;
                    }))
                continue;

            any = true;
            best = Math.Max(best, r.Score);
        }

        var score = any ? best : 0;

        var iqAnchor = quality.IqsRanges.Count == 0 ? 0 : quality.IqsRanges.Max(x => x.CropSeasonStart);
        if (iqAnchor > 0 && history.QualityBySeason.TryGetValue(iqAnchor, out var current))
        {
            if (iqAnchor >= quality.NtrmCropSeasonStart && current.HadNtrm)
                score += quality.NtrmScore;
            if (iqAnchor >= quality.MixtureCropSeasonStart && current.HadQualityMixture)
                score += quality.MixtureScore;
        }

        return score;
    }

    private static int ScoreFinancial(SegmentationConfigurationFinancial financial, FarmerKpiHistory history)
    {
        var best = int.MinValue;
        var any = false;

        foreach (var r in financial.SelfFundingRanges)
        {
            var anchor = r.CropSeasonStart;
            if (!WindowValuesSatisfied(
                    anchor,
                    r.CropSeasonAmount,
                    r.SkippedCropSeasons.Select(s => s.CropSeasonId).ToHashSet(),
                    season =>
                    {
                        if (!history.FinancialBySeason.TryGetValue(season, out var f))
                            return false;
                        return f.SelfFundingPercentage >= r.Minimum && f.SelfFundingPercentage <= r.Maximum;
                    }))
                continue;

            any = true;
            best = Math.Max(best, r.Score);
        }

        var score = any ? best : 0;

        var finAnchor = financial.SelfFundingRanges.Count == 0
            ? 0
            : financial.SelfFundingRanges.Max(x => x.CropSeasonStart);
        if (finAnchor > 0
            && history.FinancialBySeason.TryGetValue(finAnchor, out var cur)
            && cur.HaveDebt
            && finAnchor >= financial.DebtCropSeason)
            score += financial.DebtScore;

        return score;
    }

    private static int ScoreTechnology(SegmentationConfigurationTechnology technology, FarmerKpiHistory history)
    {
        var sum = 0;

        if (history.TechnologiesBySeason.TryGetValue(technology.HasLargeBaseRidgeWithMulchCropSeason, out var tMulch)
            && tMulch.HasLargeBaseRidgeWithMulch)
            sum += technology.HasLargeBaseRidgeWithMulchScore;

        if (history.TechnologiesBySeason.TryGetValue(technology.HasBroadGrateFurnaceCropSeason, out var tFurnace)
            && tFurnace.HasBroadGrateFurnace)
            sum += technology.HasBroadGrateFurnaceScore;

        if (history.TechnologiesBySeason.TryGetValue(technology.HasTechnologyPackageAdherenceCropSeason, out var tPkg)
            && tPkg.HasTechnologyPackageAdherence)
            sum += technology.HasTechnologyPackageAdherenceScore;

        return sum;
    }

    private static int ScoreEsg(SegmentationConfigurationEsg esg, FarmerKpiHistory history)
    {
        var score = 0;

        if (history.EsgBySeason.TryGetValue(esg.ReforestationCropSeason, out var kr))
        {
            var add = kr.ReforestationPercentage * esg.ReforestationScorePerPercentualPoint;
            score += Math.Clamp(add, 0, esg.ReforestationMaximumScore);
        }

        if (history.EsgBySeason.TryGetValue(esg.NativeForestCropSeason, out var kn))
        {
            var add = kn.NativeForestPercentage * esg.NativeForestScorePerPercentualPoint;
            score += Math.Clamp(add, 0, esg.NativeForestMaximumScore);
        }

        if (history.EsgBySeason.TryGetValue(esg.MinorIrregularityCropSeason, out var km) && km.HasMinorIrregularity)
            score += esg.MinorIrregularityScore;
        if (history.EsgBySeason.TryGetValue(esg.MajorIrregularityCropSeason, out var kM) && kM.HasMajorIrregularity)
            score += esg.MajorIrregularityScore;

        return score;
    }

    private static int ScoreYield(SegmentationConfigurationYield yield, FarmerKpiHistory history)
    {
        var best = int.MinValue;
        var any = false;

        foreach (var r in yield.Ranges)
        {
            var anchor = r.CropSeasonStart;
            if (!WindowValuesSatisfied(
                    anchor,
                    r.CropSeasonAmount,
                    r.SkippedCropSeasons.Select(s => s.CropSeasonId).ToHashSet(),
                    season =>
                    {
                        if (!history.YieldBySeason.TryGetValue(season, out var y))
                            return false;
                        return y >= r.Minimum && y <= r.Maximum;
                    }))
                continue;

            any = true;
            best = Math.Max(best, r.Score);
        }

        return any ? best : 0;
    }

    private static int ScoreScale(SegmentationConfigurationScale scale, FarmerKpiHistory history)
    {
        var best = int.MinValue;
        var any = false;

        foreach (var r in scale.Ranges)
        {
            var anchor = r.CropSeasonStart;
            if (!WindowValuesSatisfied(
                    anchor,
                    r.CropSeasonAmount,
                    r.SkippedCropSeasons.Select(s => s.CropSeasonId).ToHashSet(),
                    season =>
                    {
                        if (!history.ScaleBySeason.TryGetValue(season, out var s))
                            return false;
                        return s >= r.Minimum && s <= r.Maximum;
                    }))
                continue;

            any = true;
            best = Math.Max(best, r.Score);
        }

        return any ? best : 0;
    }

    /// <summary>
    /// Every season in <c>[anchorSeason - cropSeasonAmount + 1, anchorSeason]</c> (excluding skipped ids) must satisfy <paramref name="predicate"/>.
    /// </summary>
    private static bool WindowValuesSatisfied(
        int anchorSeason,
        int cropSeasonAmount,
        HashSet<int> skippedSeasons,
        Func<int, bool> predicate)
    {
        if (cropSeasonAmount <= 0)
            return false;

        var from = anchorSeason - cropSeasonAmount + 1;
        for (var season = from; season <= anchorSeason; season++)
        {
            if (skippedSeasons.Contains(season))
                continue;
            if (!predicate(season))
                return false;
        }

        return true;
    }
}
