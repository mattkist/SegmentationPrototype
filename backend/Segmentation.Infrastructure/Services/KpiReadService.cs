using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class KpiReadService(AppDbContext db) : IKpiReadService
{
    public async Task<IReadOnlyList<LoyaltyKpiRowDto>> ListLoyaltyAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.LoyaltyKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new LoyaltyKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.DeliveredPercentage))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QualityKpiRowDto>> ListQualityAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.QualityKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new QualityKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.Iqs, k.HadNtrm, k.HadQualityMixture))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialKpiRowDto>> ListFinancialAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.FinancialKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new FinancialKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.SelfFundingPercentage, k.HaveDebt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<YieldKpiRowDto>> ListYieldAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.YieldKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new YieldKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.Yield))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScaleKpiRowDto>> ListScaleAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.ScaleKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new ScaleKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.Scale))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.TechnologiesKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new TechnologiesKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.HasLargeBaseRidgeWithMulch, k.HasBroadGrateFurnace, k.HasTechnologyPackageAdherence, k.HasStandardBarn))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EsgKpiRowDto>> ListEsgAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default)
    {
        var query = db.EsgKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new EsgKpiRowDto(k.Farmer.Code, k.CropSeasonId, k.CropSeason.Code, k.CultureTypeCode, k.ReforestationPercentage, k.NativeForestPercentage, k.HasMinorIrregularity, k.HasMajorIrregularity))
            .ToListAsync(cancellationToken);
    }
}
