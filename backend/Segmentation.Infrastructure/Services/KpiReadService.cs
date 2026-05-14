using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class KpiReadService(AppDbContext db) : IKpiReadService
{
    public async Task<IReadOnlyList<LoyaltyKpiRowDto>> ListLoyaltyAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.LoyaltyKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new LoyaltyKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.DeliveredPercentage))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<QualityKpiRowDto>> ListQualityAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.QualityKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new QualityKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.Iqs,
                k.HadNtrm,
                k.HadQualityMixture))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FinancialKpiRowDto>> ListFinancialAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.FinancialKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new FinancialKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.SelfFundingPercentage,
                k.HaveDebt))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<YieldKpiRowDto>> ListYieldAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.YieldKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new YieldKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.Yield))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ScaleKpiRowDto>> ListScaleAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.ScaleKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new ScaleKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.Scale))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.TechnologiesKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new TechnologiesKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.HasLargeBaseRidgeWithMulch,
                k.HasBroadGrateFurnace,
                k.HasTechnologyPackageAdherence))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EsgKpiRowDto>> ListEsgAsync(int cropSeasonId, CancellationToken cancellationToken = default) =>
        await db.EsgKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .Include(k => k.Farmer)
            .Include(k => k.CropSeason)
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new EsgKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.ReforestationPercentage,
                k.NativeForestPercentage,
                k.HasMinorIrregularity,
                k.HasMajorIrregularity))
            .ToListAsync(cancellationToken);
}
