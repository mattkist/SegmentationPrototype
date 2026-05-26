using System.Globalization;
using System.Text;
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

        ValidateKpiScopes(dto.KpiScopes);

        var allSeasonIds = dto.KpiScopes.SelectMany(s => s.CropSeasonIds).Distinct().ToList();
        var knownSeasons = await db.CropSeasons.AsNoTracking()
            .Where(c => allSeasonIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        var missing = allSeasonIds.Except(knownSeasons).ToList();
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

        var scopeSet = SimulationKpiScopeMapper.ToScopeSet(dto.KpiScopes);
        var scaleSeasonIds = scopeSet.Scale.CropSeasonIdsDescending;

        var scaleByFarmerCulture = await LoadScaleTotalsByFarmerCultureAsync(
            farmers.Select(f => f.Id).ToList(), scaleSeasonIds, cancellationToken);

        var bundlesByCulture = configuration.CultureTypes.ToDictionary(
            ct => ct.CultureTypeCode,
            BuildScoringBundle);

        var scoringContext = new SimulationScoringContext
        {
            TargetCropSeasonId = dto.CropSeasonId,
            KpiScopes = scopeSet
        };

        var priorSeasonId = dto.CropSeasonId - 1;
        var simId = Guid.NewGuid();
        var rows = new List<SegmentationSimulationFarmer>(farmers.Count);

        foreach (var farmer in farmers)
        {
            var cultureTypeCode = ResolveCultureTypeCode(farmer.Id, scaleByFarmerCulture, configuration.CultureTypes);
            if (cultureTypeCode is null)
                continue;

            if (!bundlesByCulture.TryGetValue(cultureTypeCode, out var bundle))
                continue;

            if (!await HasPriorSeasonContractAsync(farmer.Id, cultureTypeCode, priorSeasonId, cancellationToken))
            {
                rows.Add(NewFarmerRow(simId, farmer, cultureTypeCode));
                continue;
            }

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
                TotalScore = total,
                NonExclusiveFarmer = farmer.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = segment?.Id,
                Rank = 0,
                IsNewFarmer = false
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
            KpiScopes = SimulationKpiScopeMapper.ToEntities(simId, dto.KpiScopes),
            Farmers = rows
        };

        db.SegmentationSimulations.Add(simulation);
        await db.SaveChangesAsync(cancellationToken);

        return (await MapDetailAsync(simId, cancellationToken))!;
    }

    private static void ValidateKpiScopes(IReadOnlyList<SimulationKpiScopeInputDto> scopes)
    {
        if (scopes.Count == 0)
            throw new ArgumentException("At least one KPI scope is required.");

        var kinds = scopes.Select(s => s.KpiKind).ToList();
        var missing = SimulationKpiKind.All.Except(kinds, StringComparer.OrdinalIgnoreCase).ToList();
        if (missing.Count > 0)
            throw new ArgumentException($"Missing KPI scope(s): {string.Join(", ", missing)}.");

        foreach (var scope in scopes)
        {
            if (scope.CropSeasonIds.Count == 0)
                throw new ArgumentException($"KPI scope '{scope.KpiKind}' requires at least one crop season.");
        }
    }

    private static SegmentationSimulationFarmer NewFarmerRow(Guid simId, Farmer farmer, string cultureTypeCode) =>
        new()
        {
            Id = Guid.NewGuid(),
            SegmentationSimulationId = simId,
            FarmerId = farmer.Id,
            CultureTypeCode = cultureTypeCode,
            LoyaltyScore = 0,
            QualityScore = 0,
            FinancialScore = 0,
            TechnologiesScore = 0,
            EsgScore = 0,
            YieldScore = 0,
            ScaleScore = 0,
            TotalScore = 0,
            NonExclusiveFarmer = farmer.NonExclusiveFarmer,
            SegmentationConfigurationSegmentId = null,
            Rank = 0,
            IsNewFarmer = true
        };

    private async Task<bool> HasPriorSeasonContractAsync(
        Guid farmerId,
        string cultureTypeCode,
        int priorSeasonId,
        CancellationToken cancellationToken) =>
        await db.FarmerContractKpis.AsNoTracking()
            .AnyAsync(
                k => k.FarmerId == farmerId
                     && k.CultureTypeCode == cultureTypeCode
                     && k.CropSeasonId == priorSeasonId,
                cancellationToken);

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

    private static CultureTypeScoringBundle BuildScoringBundle(SegmentationConfigurationCultureType ct) =>
        new()
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
            SegmentThresholds = ct.CultureTypeSegments
                .Select(cs => new SegmentThreshold(
                    cs.SegmentationSegmentId,
                    cs.RangeMin,
                    cs.Segment.OnlyExclusiveFarmer))
                .ToList()
        };

    private async Task<HashSet<Guid>> GetFarmerIdsWithAnyKpiAsync(CancellationToken cancellationToken)
    {
        var ids = new HashSet<Guid>();
        ids.UnionWith(await db.FarmerContractKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.TechnologiesKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        ids.UnionWith(await db.EsgIrregularityKpis.Select(k => k.FarmerId).Distinct().ToListAsync(cancellationToken));
        return ids;
    }

    private async Task<Dictionary<Guid, Dictionary<string, int>>> LoadScaleTotalsByFarmerCultureAsync(
        IReadOnlyList<Guid> farmerIds,
        IReadOnlyList<int> scopeSeasonIds,
        CancellationToken cancellationToken)
    {
        if (scopeSeasonIds.Count == 0)
            return new Dictionary<Guid, Dictionary<string, int>>();

        var rows = await db.FarmerContractKpis.AsNoTracking()
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
            .Include(s => s.KpiScopes).ThenInclude(k => k.Seasons)
            .OrderByDescending(s => s.SimulationDate)
            .AsQueryable();

        if (cropSeasonId is { } cs)
            query = query.Where(s => s.CropSeasonId == cs);

        var list = await query.ToListAsync(cancellationToken);
        return list.Select(s => new SegmentationSimulationSummaryDto
        {
            Id = s.Id,
            SegmentationConfigurationId = s.SegmentationConfigurationId,
            ConfigurationName = s.SegmentationConfiguration.Name,
            CropSeasonId = s.CropSeasonId,
            CropSeasonCode = s.CropSeason.Code,
            KpiScopes = SimulationKpiScopeMapper.ToInputs(s.KpiScopes),
            SimulationDate = s.SimulationDate,
            Status = s.Status,
            FarmerCount = s.Farmers.Count
        }).ToList();
    }

    public Task<SegmentationSimulationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        MapDetailAsync(id, cancellationToken);

    public async Task<byte[]?> ExportCsvAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var detail = await GetAsync(id, cancellationToken);
        if (detail is null)
            return null;

        var sb = new StringBuilder();
        sb.AppendLine(
            "FarmerCode,FarmerName,CultureTypeCode,TotalScore,LoyaltyScore,QualityScore,FinancialScore,TechnologiesScore,EsgScore,YieldScore,ScaleScore,SegmentName,IsNewFarmer,NonExclusiveFarmer");

        foreach (var f in detail.Farmers)
        {
            sb.Append(CsvEscape(f.FarmerCode)).Append(',');
            sb.Append(CsvEscape(f.FarmerName)).Append(',');
            sb.Append(CsvEscape(f.CultureTypeCode)).Append(',');
            sb.Append(f.TotalScore).Append(',');
            sb.Append(f.LoyaltyScore).Append(',');
            sb.Append(f.QualityScore).Append(',');
            sb.Append(f.FinancialScore).Append(',');
            sb.Append(f.TechnologiesScore).Append(',');
            sb.Append(f.EsgScore).Append(',');
            sb.Append(f.YieldScore).Append(',');
            sb.Append(f.ScaleScore).Append(',');
            sb.Append(CsvEscape(f.SegmentName ?? string.Empty)).Append(',');
            sb.Append(f.IsNewFarmer ? "true" : "false").Append(',');
            sb.Append(f.NonExclusiveFarmer ? "true" : "false");
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        return value;
    }

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

        foreach (var row in simulation.Farmers.Where(f => !f.IsNewFarmer))
        {
            db.FarmerSegmentations.Add(new FarmerSegmentation
            {
                Id = Guid.NewGuid(),
                FarmerId = row.FarmerId,
                CropSeasonId = simulation.CropSeasonId,
                SegmentationConfigurationId = simulation.SegmentationConfigurationId,
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
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Technology)!.ThenInclude(t => t!.TechnologyScores)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Esg)!.ThenInclude(e => e!.IrregularityScores)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Yield)!.ThenInclude(y => y!.Ranges)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Scale)!.ThenInclude(s => s!.Ranges)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private async Task<FarmerKpiHistory> BuildHistoryAsync(
        Guid farmerId,
        string cultureTypeCode,
        CancellationToken cancellationToken)
    {
        var farmer = await db.Farmers.AsNoTracking().FirstAsync(f => f.Id == farmerId, cancellationToken);

        var contractRows = await db.FarmerContractKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);

        var loyalty = contractRows.ToDictionary(
            k => k.CropSeasonId,
            k => new LoyaltyKpiSnapshot(k.DeliveredAmountKg, k.ContractedAmountKg));

        var quality = contractRows.ToDictionary(
            k => k.CropSeasonId,
            k => new QualityKpiSnapshot(k.Iqs, k.HadNtrm, k.HadQualityMixture));

        var financial = contractRows.ToDictionary(
            k => k.CropSeasonId,
            k => new FinancialKpiSnapshot(k.SelfFundingPercentage, k.HaveDebt));

        var contractBySeason = contractRows.ToDictionary(
            k => k.CropSeasonId,
            k => new ContractKpiSnapshot(k.Yield, k.Scale, k.ContractedAmountKg));

        var techRows = await db.TechnologiesKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var technologies = techRows
            .GroupBy(k => k.CropSeasonId)
            .ToDictionary(
                g => g.Key,
                g => new TechnologiesKpiSnapshot(g.Select(x => x.TechnologyId).ToHashSet()));

        var irregularityRows = await db.EsgIrregularityKpis.AsNoTracking()
            .Where(k => k.FarmerId == farmerId && k.CultureTypeCode == cultureTypeCode)
            .ToListAsync(cancellationToken);
        var irregularitiesBySeason = irregularityRows
            .GroupBy(k => k.CropSeasonId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.IrregularityTypeId).ToHashSet());

        var esg = contractRows.ToDictionary(
            k => k.CropSeasonId,
            k => new EsgKpiSnapshot(
                k.ReforestationPercentage,
                k.NativeForestPercentage,
                irregularitiesBySeason.GetValueOrDefault(k.CropSeasonId) ?? []));

        return new FarmerKpiHistory
        {
            FarmerId = farmerId,
            NonExclusiveFarmer = farmer.NonExclusiveFarmer,
            LoyaltyBySeason = loyalty,
            QualityBySeason = quality,
            FinancialBySeason = financial,
            TechnologiesBySeason = technologies,
            EsgBySeason = esg,
            ContractBySeason = contractBySeason
        };
    }

    private async Task<SegmentationSimulationDetailDto?> MapDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var s = await db.SegmentationSimulations.AsNoTracking()
            .Include(x => x.SegmentationConfiguration)
            .Include(x => x.CropSeason)
            .Include(x => x.KpiScopes).ThenInclude(k => k.Seasons)
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
                NonExclusiveFarmer = f.NonExclusiveFarmer,
                SegmentationConfigurationSegmentId = f.SegmentationConfigurationSegmentId,
                SegmentName = f.Segment != null ? f.Segment.SegmentName : null,
                IsNewFarmer = f.IsNewFarmer
            })
            .ToList();

        var scored = farmers.Where(f => !f.IsNewFarmer).ToList();
        var overall = BuildSegmentDistribution(scored);
        var byCulture = scored
            .GroupBy(f => f.CultureTypeCode)
            .OrderBy(g => g.Key)
            .Select(g => new CultureTypeSegmentDistributionDto
            {
                CultureTypeCode = g.Key,
                Segments = BuildSegmentDistribution(g.ToList())
            })
            .ToList();

        return new SegmentationSimulationDetailDto
        {
            Id = s.Id,
            SegmentationConfigurationId = s.SegmentationConfigurationId,
            ConfigurationName = s.SegmentationConfiguration.Name,
            CropSeasonId = s.CropSeasonId,
            CropSeasonCode = s.CropSeason.Code,
            KpiScopes = SimulationKpiScopeMapper.ToInputs(s.KpiScopes),
            SimulationDate = s.SimulationDate,
            Status = s.Status,
            Farmers = farmers,
            OverallSegmentDistribution = overall,
            SegmentDistributionByCultureType = byCulture
        };
    }

    private static IReadOnlyList<SegmentShareDto> BuildSegmentDistribution(
        IReadOnlyList<SegmentationSimulationFarmerDto> farmers)
    {
        if (farmers.Count == 0)
            return [];

        var total = farmers.Count;
        return farmers
            .GroupBy(f => f.SegmentName ?? "(No segment)")
            .OrderByDescending(g => g.Count())
            .Select(g => new SegmentShareDto
            {
                SegmentName = g.Key,
                FarmerCount = g.Count(),
                Percentage = Math.Round(g.Count() * 100m / total, 2)
            })
            .ToList();
    }
}
