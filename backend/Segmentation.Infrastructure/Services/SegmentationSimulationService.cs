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

        var configuration = await LoadConfigurationGraphAsync(dto.SegmentationConfigurationId, cancellationToken);
        if (configuration is null)
            throw new KeyNotFoundException($"Segmentation configuration '{dto.SegmentationConfigurationId}' was not found.");

        if (configuration.Segments.Count == 0)
            throw new InvalidOperationException("The segmentation configuration has no segments.");

        if (configuration.Loyalty is null || configuration.Quality is null || configuration.Financial is null
            || configuration.Technology is null || configuration.Esg is null || configuration.Yield is null
            || configuration.Scale is null)
            throw new InvalidOperationException("The segmentation configuration is missing one or more KPI blocks.");

        var farmers = await db.Farmers.AsNoTracking().OrderBy(f => f.Code).ToListAsync(cancellationToken);
        var histories = await BuildHistoriesAsync(farmers.Select(f => f.Id).ToList(), cancellationToken);

        var simId = Guid.NewGuid();
        var rows = new List<SegmentationSimulationFarmer>(farmers.Count);
        var scoreTuples = new List<(Guid FarmerId, int Total)>(farmers.Count);

        foreach (var farmer in farmers)
        {
            var history = histories[farmer.Id];
            var scores = SimulationFarmerScoring.ComputeScores(configuration, history);
            var total = scores.Total;
            var segment = SimulationFarmerScoring.PickSegment(
                configuration.Segments,
                total,
                farmer.NonExclusiveFarmer);

            rows.Add(new SegmentationSimulationFarmer
            {
                Id = Guid.NewGuid(),
                SegmentationSimulationId = simId,
                FarmerId = farmer.Id,
                LoyaltyScore = scores.Loyalty,
                QualityScore = scores.Quality,
                FinancialScore = scores.Financial,
                TechnologiesScore = scores.Technologies,
                EsgScore = scores.Esg,
                YieldScore = scores.Yield,
                ScaleScore = scores.Scale,
                TotalScore = total,
                NonExclusiveFarmer = farmer.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = segment?.Id,
                Rank = 0
            });

            scoreTuples.Add((farmer.Id, total));
        }

        var ranks = SimulationFarmerScoring.CompetitionRanks(scoreTuples);
        foreach (var row in rows)
            row.Rank = ranks[row.FarmerId];

        var simulation = new SegmentationSimulation
        {
            Id = simId,
            SegmentationConfigurationId = configuration.Id,
            CropSeasonId = dto.CropSeasonId,
            SimulationDate = DateTime.UtcNow,
            Status = "S",
            Farmers = rows
        };

        db.SegmentationSimulations.Add(simulation);
        await db.SaveChangesAsync(cancellationToken);

        return (await MapDetailAsync(simId, cancellationToken))!;
    }

    public async Task<IReadOnlyList<SegmentationSimulationSummaryDto>> ListAsync(
        int? cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var query = db.SegmentationSimulations.AsNoTracking()
            .Include(s => s.SegmentationConfiguration)
            .Include(s => s.CropSeason)
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
                NonExclusiveFarmer = row.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = row.SegmentationConfigurationSegmentId,
                Rank = row.Rank
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
            .Include(c => c.Loyalty)!.ThenInclude(l => l!.SeasonQuantityRanges).ThenInclude(r => r.SkippedCropSeasons)
            .Include(c => c.Loyalty)!.ThenInclude(l => l!.HistoricalVolumeRanges)
            .Include(c => c.Quality)!.ThenInclude(q => q!.IqsRanges).ThenInclude(r => r.SkippedCropSeasons)
            .Include(c => c.Financial)!.ThenInclude(f => f!.SelfFundingRanges).ThenInclude(r => r.SkippedCropSeasons)
            .Include(c => c.Technology)
            .Include(c => c.Esg)
            .Include(c => c.Yield)!.ThenInclude(y => y!.Ranges).ThenInclude(r => r.SkippedCropSeasons)
            .Include(c => c.Scale)!.ThenInclude(s => s!.Ranges).ThenInclude(r => r.SkippedCropSeasons)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private async Task<Dictionary<Guid, FarmerKpiHistory>> BuildHistoriesAsync(
        IReadOnlyList<Guid> farmerIds,
        CancellationToken cancellationToken)
    {
        if (farmerIds.Count == 0)
            return new Dictionary<Guid, FarmerKpiHistory>();

        var farmers = await db.Farmers.AsNoTracking()
            .Where(f => farmerIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);

        var loyaltyRows = await db.LoyaltyKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new { k.FarmerId, k.CropSeasonId, k.DeliveredPercentage })
            .ToListAsync(cancellationToken);
        var loyalty = loyaltyRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, int>)g.ToDictionary(x => x.CropSeasonId, x => x.DeliveredPercentage));

        var qualityRows = await db.QualityKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new
            {
                k.FarmerId,
                k.CropSeasonId,
                k.Iqs,
                k.HadNtrm,
                k.HadQualityMixture
            })
            .ToListAsync(cancellationToken);
        var quality = qualityRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, QualityKpiSnapshot>)g.ToDictionary(
                    x => x.CropSeasonId,
                    x => new QualityKpiSnapshot(x.Iqs, x.HadNtrm, x.HadQualityMixture)));

        var financialRows = await db.FinancialKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new { k.FarmerId, k.CropSeasonId, k.SelfFundingPercentage, k.HaveDebt })
            .ToListAsync(cancellationToken);
        var financial = financialRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, FinancialKpiSnapshot>)g.ToDictionary(
                    x => x.CropSeasonId,
                    x => new FinancialKpiSnapshot(x.SelfFundingPercentage, x.HaveDebt)));

        var technologiesRows = await db.TechnologiesKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new
            {
                k.FarmerId,
                k.CropSeasonId,
                k.HasLargeBaseRidgeWithMulch,
                k.HasBroadGrateFurnace,
                k.HasTechnologyPackageAdherence
            })
            .ToListAsync(cancellationToken);
        var technologiesByFarmer = technologiesRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, TechnologiesKpiSnapshot>)g.ToDictionary(
                    x => x.CropSeasonId,
                    x => new TechnologiesKpiSnapshot(
                        x.HasLargeBaseRidgeWithMulch,
                        x.HasBroadGrateFurnace,
                        x.HasTechnologyPackageAdherence)));

        var esgRows = await db.EsgKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new
            {
                k.FarmerId,
                k.CropSeasonId,
                k.ReforestationPercentage,
                k.NativeForestPercentage,
                k.HasMinorIrregularity,
                k.HasMajorIrregularity
            })
            .ToListAsync(cancellationToken);
        var esg = esgRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, EsgKpiSnapshot>)g.ToDictionary(
                    x => x.CropSeasonId,
                    x => new EsgKpiSnapshot(
                        x.ReforestationPercentage,
                        x.NativeForestPercentage,
                        x.HasMinorIrregularity,
                        x.HasMajorIrregularity)));

        var yieldRows = await db.YieldKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new { k.FarmerId, k.CropSeasonId, k.Yield })
            .ToListAsync(cancellationToken);
        var yieldByFarmer = yieldRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, int>)g.ToDictionary(x => x.CropSeasonId, x => x.Yield));

        var scaleRows = await db.ScaleKpis.AsNoTracking()
            .Where(k => farmerIds.Contains(k.FarmerId))
            .Select(k => new { k.FarmerId, k.CropSeasonId, k.Scale })
            .ToListAsync(cancellationToken);
        var scaleByFarmer = scaleRows.GroupBy(x => x.FarmerId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyDictionary<int, int>)g.ToDictionary(x => x.CropSeasonId, x => x.Scale));

        var dict = new Dictionary<Guid, FarmerKpiHistory>(farmerIds.Count);
        foreach (var id in farmerIds)
        {
            if (!farmers.TryGetValue(id, out var f))
                continue;

            dict[id] = new FarmerKpiHistory
            {
                FarmerId = id,
                NonExclusiveFarmer = f.NonExclusiveFarmer,
                LoyaltyDeliveredPctBySeason = loyalty.GetValueOrDefault(id) ?? new Dictionary<int, int>(),
                QualityBySeason = quality.GetValueOrDefault(id) ?? new Dictionary<int, QualityKpiSnapshot>(),
                FinancialBySeason = financial.GetValueOrDefault(id) ?? new Dictionary<int, FinancialKpiSnapshot>(),
                TechnologiesBySeason = technologiesByFarmer.GetValueOrDefault(id) ?? new Dictionary<int, TechnologiesKpiSnapshot>(),
                EsgBySeason = esg.GetValueOrDefault(id) ?? new Dictionary<int, EsgKpiSnapshot>(),
                YieldBySeason = yieldByFarmer.GetValueOrDefault(id) ?? new Dictionary<int, int>(),
                ScaleBySeason = scaleByFarmer.GetValueOrDefault(id) ?? new Dictionary<int, int>()
            };
        }

        return dict;
    }

    private async Task<SegmentationSimulationDetailDto?> MapDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var s = await db.SegmentationSimulations.AsNoTracking()
            .Include(x => x.SegmentationConfiguration)
            .Include(x => x.CropSeason)
            .Include(x => x.Farmers)
            .ThenInclude(f => f.Farmer)
            .Include(x => x.Farmers)
            .ThenInclude(f => f.Segment)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (s is null)
            return null;

        var farmers = s.Farmers
            .OrderBy(f => f.Rank)
            .ThenBy(f => f.Farmer.Code)
            .Select(f => new SegmentationSimulationFarmerDto
            {
                FarmerId = f.FarmerId,
                FarmerCode = f.Farmer.Code,
                FarmerName = f.Farmer.Name,
                TotalScore = f.TotalScore,
                LoyaltyScore = f.LoyaltyScore,
                QualityScore = f.QualityScore,
                FinancialScore = f.FinancialScore,
                TechnologiesScore = f.TechnologiesScore,
                EsgScore = f.EsgScore,
                YieldScore = f.YieldScore,
                ScaleScore = f.ScaleScore,
                NonExclusiveFarmer = f.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = f.SegmentationConfigurationSegmentId,
                SegmentName = f.Segment != null ? f.Segment.SegmentName : null,
                Rank = f.Rank
            })
            .ToList();

        return new SegmentationSimulationDetailDto
        {
            Id = s.Id,
            SegmentationConfigurationId = s.SegmentationConfigurationId,
            ConfigurationName = s.SegmentationConfiguration.Name,
            CropSeasonId = s.CropSeasonId,
            CropSeasonCode = s.CropSeason.Code,
            SimulationDate = s.SimulationDate,
            Status = s.Status,
            Farmers = farmers
        };
    }
}
