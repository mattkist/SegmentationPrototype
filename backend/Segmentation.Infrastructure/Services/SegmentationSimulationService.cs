using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Domain.Entities;
using Segmentation.Domain.Scoring;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class SegmentationSimulationService(AppDbContext db) : ISegmentationSimulationService
{
    public async Task<SegmentationSimulationDetailDto> CreateAsync(
        CreateSegmentationSimulationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!await db.CropSeasons.AnyAsync(c => c.Id == dto.CropSeasonId, cancellationToken))
            throw new KeyNotFoundException($"Crop season '{dto.CropSeasonId}' was not found.");

        var scopeIds = dto.ScopeCropSeasonIds.Distinct().OrderByDescending(x => x).ToList();
        if (scopeIds.Count == 0)
            throw new ArgumentException("At least one scope crop season is required.");

        var knownSeasons = await db.CropSeasons.AsNoTracking()
            .Where(c => scopeIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        var missing = scopeIds.Except(knownSeasons).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException($"Crop season(s) not found: {string.Join(", ", missing)}.");

        var configuration = await LoadConfigurationGraphAsync(dto.SegmentationConfigurationId, cancellationToken);
        if (configuration is null)
            throw new KeyNotFoundException($"Segmentation configuration '{dto.SegmentationConfigurationId}' was not found.");

        if (configuration.Segments.Count == 0)
            throw new InvalidOperationException("The segmentation configuration has no segments.");

        if (configuration.CultureTypes.Count == 0)
            throw new InvalidOperationException("The segmentation configuration has no culture type blocks.");

        foreach (var ct in configuration.CultureTypes)
        {
            if (ct.Loyalty is null || ct.Quality is null || ct.Financial is null
                || ct.Technology is null || ct.Esg is null || ct.Yield is null || ct.Scale is null)
                throw new InvalidOperationException(
                    $"Culture type '{ct.CultureTypeCode}' is missing one or more KPI blocks.");
        }

        var farmerIds = await GetFarmerIdsWithAnyKpiAsync(cancellationToken);
        var farmers = await db.Farmers.AsNoTracking()
            .Where(f => farmerIds.Contains(f.Id))
            .OrderBy(f => f.Code)
            .ToListAsync(cancellationToken);

        if (farmers.Count == 0)
            throw new InvalidOperationException("No farmers have KPI data. Cannot run simulation.");

        var scaleByFarmerCulture = await LoadScaleTotalsByFarmerCultureAsync(
            farmers.Select(f => f.Id).ToList(), scopeIds, cancellationToken);

        var bundlesByCulture = configuration.CultureTypes.ToDictionary(
            ct => ct.CultureTypeCode,
            BuildScoringBundle);

        var scoringContext = new SimulationScoringContext
        {
            TargetCropSeasonId = dto.CropSeasonId,
            ScopeCropSeasonIdsDescending = scopeIds
        };

        var simId = Guid.NewGuid();
        var rows = new List<SegmentationSimulationFarmer>(farmers.Count);

        foreach (var farmer in farmers)
        {
            var cultureTypeCode = ResolveCultureTypeCode(farmer.Id, scaleByFarmerCulture, configuration.CultureTypes);
            if (cultureTypeCode is null)
                continue;

            if (!bundlesByCulture.TryGetValue(cultureTypeCode, out var bundle))
                continue;

            var history = await BuildHistoryAsync(farmer.Id, cultureTypeCode, cancellationToken);
            var scores = SimulationFarmerScoring.ComputeScores(bundle, scoringContext, history);
            var total = scores.Total;
            var segment = SimulationFarmerScoring.PickSegment(
                bundle.SegmentThresholds,
                configuration.Segments,
                total,
                farmer.NonExclusiveFarmer);

            rows.Add(new SegmentationSimulationFarmer
            {
                Id = Guid.NewGuid(),
                SegmentationSimulationId = simId,
                FarmerId = farmer.Id,
                CultureTypeCode = cultureTypeCode,
                LoyaltyScore = scores.Loyalty,
                QualityScore = scores.Quality,
                FinancialScore = scores.Financial,
                TechnologiesScore = scores.Technologies,
                EsgScore = scores.Esg,
                YieldScore = scores.Yield,
                ScaleScore = scores.Scale,
                YieldAndScaleScore = scores.YieldAndScale,
                TotalScore = total,
                NonExclusiveFarmer = farmer.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = segment?.Id,
                Rank = 0
            });
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("No farmers could be scored with the configuration culture types.");

        var simulation = new SegmentationSimulation
        {
            Id = simId,
            SegmentationConfigurationId = configuration.Id,
            CropSeasonId = dto.CropSeasonId,
            SimulationDate = DateTime.UtcNow,
            Status = "S",
            ScopeCropSeasons = scopeIds.Select(cs => new SegmentationSimulationCropSeason
            {
                Id = Guid.NewGuid(),
                SegmentationSimulationId = simId,
                CropSeasonId = cs
            }).ToList(),
            Farmers = rows
        };

        db.SegmentationSimulations.Add(simulation);
        await db.SaveChangesAsync(cancellationToken);

        return (await MapDetailAsync(simId, cancellationToken))!;
    }

    private static string? ResolveCultureTypeCode(
        Guid farmerId,
        IReadOnlyDictionary<Guid, Dictionary<string, int>> scaleByFarmerCulture,
        ICollection<SegmentationConfigurationCultureType> configuredCultureTypes)
    {
        if (!scaleByFarmerCulture.TryGetValue(farmerId, out var totals) || totals.Count == 0)
            return null;

        var allowed = configuredCultureTypes.Select(ct => ct.CultureTypeCode).ToHashSet(StringComparer.Ordinal);
        return totals
            .Where(kv => allowed.Contains(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .FirstOrDefault();
    }

    private static CultureTypeScoringBundle BuildScoringBundle(SegmentationConfigurationCultureType ct)
    {
        return new CultureTypeScoringBundle
        {
            CultureTypeConfigurationId = ct.Id,
            CultureTypeCode = ct.CultureTypeCode,
            Loyalty = ct.Loyalty!,
            Quality = ct.Quality!,
            Financial = ct.Financial!,
            Technology = ct.Technology!,
            Esg = ct.Esg!,
            Yield = ct.Yield!,
            Scale = ct.Scale!,
            YieldAndScale = ct.YieldAndScale!,
            SegmentThresholds = ct.CultureTypeSegments
                .Select(cs => new SegmentThreshold(
                    cs.SegmentationSegmentId,
                    cs.RangeMin,
                    cs.Segment.OnlyExclusiveFarmer))
                .ToList()
        };
    }

    private async Task<HashSet<Guid>> GetFarmerIdsWithAnyKpiAsync(CancellationToken cancellationToken)
    {
        var ids = new HashSet<Guid>();
        ids.UnionWith(await db.LoyaltyKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.QualityKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.FinancialKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.YieldKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.ScaleKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.TechnologiesKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.EsgKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        return ids;
    }

    private async Task<Dictionary<Guid, Dictionary<string, int>>> LoadScaleTotalsByFarmerCultureAsync(
        IReadOnlyList<Guid> farmerIds,
        IReadOnlyList<int> scopeSeasonIds,
        CancellationToken cancellationToken)
    {
        var rows = await db.ScaleKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId) && scopeSeasonIds.Contains(k.CropSeasonId))
            .Select(k => new { k.FarmerId, k.CultureTypeCode, k.Scale })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x => x.CultureTypeCode)
                    .ToDictionary(cg => cg.Key, cg => cg.Sum(x => x.Scale), StringComparer.Ordinal));
    }

    public async Task<IReadOnlyList<SegmentationSimulationSummaryDto>> ListAsync(
        int? cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var query = db.SegmentationSimulations.AsNoTracking()
            .Include(s => s.SegmentationConfiguration)
            .Include(s => s.CropSeason)
            .Include(s => s.ScopeCropSeasons)
            .OrderByDescending(s => s.SimulationDate)
            .AsQueryable();

        if (cropSeasonId is { } cs)
            query = query.Where(s => s.CropSeasonId == cs);

        return await query
            .Select(s => new SegmentationSimulationSummaryDto
            {
                Id = s.Id,
                SegmentationConfigurationId = s.SegmentationConfigurationId,
                ConfigurationName = s.SegmentationConfiguration.Name,
                CropSeasonId = s.CropSeasonId,
                CropSeasonCode = s.CropSeason.Code,
                ScopeCropSeasonIds = s.ScopeCropSeasons.Select(x => x.CropSeasonId).OrderByDescending(x => x).ToList(),
                SimulationDate = s.SimulationDate,
                Status = s.Status,
                FarmerCount = s.Farmers.Count
            })
            .ToListAsync(cancellationToken);
    }

    public Task<SegmentationSimulationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        MapDetailAsync(id, cancellationToken);

    public async Task AcceptOfficialAsync(Guid simulationId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var simulation = await db.SegmentationSimulations
            .Include(s => s.Farmers)
            .FirstOrDefaultAsync(s => s.Id == simulationId, cancellationToken);

        if (simulation is null)
            throw new KeyNotFoundException($"Segmentation simulation '{simulationId}' was not found.");

        if (simulation.Status != "S")
            throw new InvalidOperationException("Only simulations with status S can be accepted as official.");

        var othersOfficial = await db.SegmentationSimulations
            .Where(s => s.CropSeasonId == simulation.CropSeasonId && s.Id != simulation.Id && s.Status == "O")
            .ToListAsync(cancellationToken);

        foreach (var o in othersOfficial)
            o.Status = "S";

        simulation.Status = "O";

        var toRemove = await db.FarmerSegmentations
            .Where(x => x.CropSeasonId == simulation.CropSeasonId)
            .ToListAsync(cancellationToken);
        db.FarmerSegmentations.RemoveRange(toRemove);

        foreach (var row in simulation.Farmers)
        {
            db.FarmerSegmentations.Add(new FarmerSegmentation
            {
                Id = Guid.NewGuid(),
                FarmerId = row.FarmerId,
                CropSeasonId = simulation.CropSeasonId,
                TotalScore = row.TotalScore,
                LoyaltyScore = row.LoyaltyScore,
                QualityScore = row.QualityScore,
                FinancialScore = row.FinancialScore,
                TechnologiesScore = row.TechnologiesScore,
                EsgScore = row.EsgScore,
                YieldScore = row.YieldScore,
                ScaleScore = row.ScaleScore,
                YieldAndScaleScore = row.YieldAndScaleScore,
                NonExclusiveFarmer = row.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = row.SegmentationConfigurationSegmentId,
                Rank = 0
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<SegmentationConfiguration?> LoadConfigurationGraphAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.SegmentationConfigurations
            .AsNoTracking()
            .Include(c => c.Segments)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.CultureTypeSegments).ThenInclude(cs => cs.Segment)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Loyalty)!.ThenInclude(l => l!.SeasonQuantityRanges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Loyalty)!.ThenInclude(l => l!.HistoricalVolumeRanges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Quality)!.ThenInclude(q => q!.IqsRanges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Financial)!.ThenInclude(f => f!.SelfFundingRanges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Technology)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Esg)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Yield)!.ThenInclude(y => y!.Ranges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Scale)!.ThenInclude(s => s!.Ranges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.YieldAndScale)!.ThenInclude(ys => ys!.Ranges)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private async Task<FarmerKpiHistory> BuildHistoryAsync(
        Guid farmerId,
        string cultureTypeCode,
        CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.AsNoTracking().FirstAsync(f => f.Id == farmerId, cancellationToken);

        var loyalty = await db.LoyaltyKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToDictionaryAsync(k => k.CropSeasonId, k => k.DeliveredPercentage, cancellationToken);

        var qualityRows = await db.QualityKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var quality = qualityRows.ToDictionary(
            k => k.CropSeasonId,
            k => new QualityKpiSnapshot(k.Iqs, k.HadNtrm, k.HadQualityMixture));

        var financialRows = await db.FinancialKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var financial = financialRows.ToDictionary(
            k => k.CropSeasonId,
            k => new FinancialKpiSnapshot(k.SelfFundingPercentage, k.HaveDebt));

        var techRows = await db.TechnologiesKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var technologies = techRows.ToDictionary(
            k => k.CropSeasonId,
            k => new TechnologiesKpiSnapshot(
                k.HasLargeBaseRidgeWithMulch,
                k.HasBroadGrateFurnace,
                k.HasTechnologyPackageAdherence,
                k.HasStandardBarn));

        var esgRows = await db.EsgKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var esg = esgRows.ToDictionary(
            k => k.CropSeasonId,
            k => new EsgKpiSnapshot(
                k.ReforestationPercentage,
                k.NativeForestPercentage,
                k.HasMinorIrregularity,
                k.HasMajorIrregularity));

        var yield = await db.YieldKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToDictionaryAsync(k => k.CropSeasonId, k => k.Yield, cancellationToken);

        var scale = await db.ScaleKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToDictionaryAsync(k => k.CropSeasonId, k => k.Scale, cancellationToken);

        return new FarmerKpiHistory
        {
            FarmerId = farmerId,
            NonExclusiveFarmer = farmer.NonExclusiveFarmer,
            LoyaltyDeliveredPctBySeason = loyalty,
            QualityBySeason = quality,
            FinancialBySeason = financial,
            TechnologiesBySeason = technologies,
            EsgBySeason = esg,
            YieldBySeason = yield,
            ScaleBySeason = scale
        };
    }

    private async Task<SegmentationSimulationDetailDto?> MapDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var s = await db.SegmentationSimulations.AsNoTracking()
            .Include(x => x.SegmentationConfiguration)
            .Include(x => x.CropSeason)
            .Include(x => x.ScopeCropSeasons)
            .Include(x => x.Farmers)
            .ThenInclude(f => f.Farmer)
            .Include(x => x.Farmers)
            .ThenInclude(f => f.Segment)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (s is null)
            return null;

        var farmers = s.Farmers
            .OrderByDescending(f => f.TotalScore)
            .ThenBy(f => f.Farmer.Code)
            .Select(f => new SegmentationSimulationFarmerDto
            {
                FarmerId = f.FarmerId,
                FarmerCode = f.Farmer.Code,
                FarmerName = f.Farmer.Name,
                CultureTypeCode = f.CultureTypeCode,
                TotalScore = f.TotalScore,
                LoyaltyScore = f.LoyaltyScore,
                QualityScore = f.QualityScore,
                FinancialScore = f.FinancialScore,
                TechnologiesScore = f.TechnologiesScore,
                EsgScore = f.EsgScore,
                YieldScore = f.YieldScore,
                ScaleScore = f.ScaleScore,
                YieldAndScaleScore = f.YieldAndScaleScore,
                NonExclusiveFarmer = f.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = f.SegmentationConfigurationSegmentId,
                SegmentName = f.Segment != null ? f.Segment.SegmentName : null
            })
            .ToList();

        return new SegmentationSimulationDetailDto
        {
            Id = s.Id,
            SegmentationConfigurationId = s.SegmentationConfigurationId,
            ConfigurationName = s.SegmentationConfiguration.Name,
            CropSeasonId = s.CropSeasonId,
            CropSeasonCode = s.CropSeason.Code,
            ScopeCropSeasonIds = s.ScopeCropSeasons.Select(x => x.CropSeasonId).OrderByDescending(x => x).ToList(),
            SimulationDate = s.SimulationDate,
            Status = s.Status,
            Farmers = farmers
        };
    }
}
