using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class FarmerReadService(AppDbContext db) : IFarmerReadService
{
    public async Task<IReadOnlyList<FarmerListItemDto>> ListForCropSeasonAsync(
        int cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var farmers = await db.Farmers.AsNoTracking()
            .OrderBy(f => f.Code)
            .ToListAsync(cancellationToken);

        var segmentations = await db.FarmerSegmentations.AsNoTracking()
            .Include(s => s.Segment)
            .Where(s => s.CropSeasonId == cropSeasonId)
            .ToDictionaryAsync(s => s.FarmerId, cancellationToken);

        return farmers.Select(f =>
        {
            if (!segmentations.TryGetValue(f.Id, out var seg))
            {
                return new FarmerListItemDto(
                    f.Id,
                    f.Code,
                    f.Name,
                    f.NonExclusiveFarmer,
                    null,
                    null,
                    null,
                    null);
            }

            return new FarmerListItemDto(
                f.Id,
                f.Code,
                f.Name,
                f.NonExclusiveFarmer,
                seg.TotalScore,
                seg.Rank,
                seg.Segment?.SegmentName,
                seg.SegmentationConfigurationSegmentId);
        }).ToList();
    }

    public async Task<FarmerDetailDto?> GetDetailAsync(
        Guid farmerId,
        int cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var farmer = await db.Farmers.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == farmerId, cancellationToken);
        if (farmer is null)
            return null;

        return await BuildDetailAsync(farmer.Id, farmer.Code, farmer.Name, farmer.NonExclusiveFarmer, cropSeasonId, cancellationToken);
    }

    public async Task<FarmerDetailDto?> GetDetailByCodeAsync(
        string farmerCode,
        int cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var normalized = farmerCode.Trim();
        var farmer = await db.Farmers.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Code == normalized, cancellationToken);
        if (farmer is null)
            return null;

        return await BuildDetailAsync(farmer.Id, farmer.Code, farmer.Name, farmer.NonExclusiveFarmer, cropSeasonId, cancellationToken);
    }

    private async Task<FarmerDetailDto?> BuildDetailAsync(
        Guid farmerId,
        string farmerCode,
        string farmerName,
        bool nonExclusiveFarmer,
        int cropSeasonId,
        CancellationToken cancellationToken)
    {
        var season = await db.CropSeasons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cropSeasonId, cancellationToken);
        if (season is null)
            return null;

        var seg = await db.FarmerSegmentations.AsNoTracking()
            .Include(s => s.Segment)
            .FirstOrDefaultAsync(s => s.FarmerId == farmerId && s.CropSeasonId == cropSeasonId, cancellationToken);

        OfficialSegmentationDto? official = seg is null
            ? null
            : new OfficialSegmentationDto(
                seg.TotalScore,
                seg.Rank,
                seg.Segment?.SegmentName,
                seg.SegmentationConfigurationSegmentId,
                seg.LoyaltyScore,
                seg.QualityScore,
                seg.FinancialScore,
                seg.TechnologiesScore,
                seg.EsgScore,
                seg.YieldScore,
                seg.ScaleScore,
                seg.YieldAndScaleScore);

        var loyalty = await db.LoyaltyKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var quality = await db.QualityKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var financial = await db.FinancialKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var yield = await db.YieldKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var scale = await db.ScaleKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var tech = await db.TechnologiesKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);
        var esg = await db.EsgKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);

        var kpis = new FarmerKpisForSeasonDto(
            loyalty is null
                ? null
                : new LoyaltyKpiRowDto(farmerCode, season.Id, season.Code, loyalty.CultureTypeCode, loyalty.DeliveredPercentage),
            quality is null
                ? null
                : new QualityKpiRowDto(
                    farmerCode,
                    season.Id,
                    season.Code,
                    quality.CultureTypeCode,
                    quality.Iqs,
                    quality.HadNtrm,
                    quality.HadQualityMixture),
            financial is null
                ? null
                : new FinancialKpiRowDto(
                    farmerCode,
                    season.Id,
                    season.Code,
                    financial.CultureTypeCode,
                    financial.SelfFundingPercentage,
                    financial.HaveDebt),
            yield is null ? null : new YieldKpiRowDto(farmerCode, season.Id, season.Code, yield.CultureTypeCode, yield.Yield),
            scale is null ? null : new ScaleKpiRowDto(farmerCode, season.Id, season.Code, scale.CultureTypeCode, scale.Scale),
            tech is null
                ? null
                : new TechnologiesKpiRowDto(
                    farmerCode,
                    season.Id,
                    season.Code,
                    tech.CultureTypeCode,
                    tech.HasLargeBaseRidgeWithMulch,
                    tech.HasBroadGrateFurnace,
                    tech.HasTechnologyPackageAdherence,
                    tech.HasStandardBarn),
            esg is null
                ? null
                : new EsgKpiRowDto(
                    farmerCode,
                    season.Id,
                    season.Code,
                    esg.CultureTypeCode,
                    esg.ReforestationPercentage,
                    esg.NativeForestPercentage,
                    esg.HasMinorIrregularity,
                    esg.HasMajorIrregularity));

        return new FarmerDetailDto(
            farmerId,
            farmerCode,
            farmerName,
            nonExclusiveFarmer,
            season.Id,
            season.Code,
            official,
            kpis);
    }
}
