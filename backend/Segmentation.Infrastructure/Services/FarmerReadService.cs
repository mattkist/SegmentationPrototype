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
                seg.ScaleScore);

        var contract = await db.FarmerContractKpis.AsNoTracking()
            .FirstOrDefaultAsync(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId, cancellationToken);

        var techRows = await db.TechnologiesKpis.AsNoTracking()
            .Include(k => k.Technology)
            .Where(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId)
            .OrderBy(k => k.TechnologyId)
            .ToListAsync(cancellationToken);

        var esgIrregularities = await db.EsgIrregularityKpis.AsNoTracking()
            .Include(k => k.IrregularityType)
            .Where(k => k.FarmerId == farmerId && k.CropSeasonId == cropSeasonId)
            .OrderBy(k => k.IrregularityTypeId)
            .ToListAsync(cancellationToken);

        FarmerContractKpiRowDto? contractDto = contract is null
            ? null
            : new FarmerContractKpiRowDto(
                farmerCode,
                season.Id,
                season.Code,
                contract.CultureTypeCode,
                contract.DeliveredPercentage,
                contract.DeliveredAmountKg,
                contract.ContractedAmountKg,
                contract.Iqs,
                contract.HadNtrm,
                contract.HadQualityMixture,
                contract.SelfFundingPercentage,
                contract.HaveDebt,
                contract.Yield,
                contract.Scale,
                contract.ReforestationPercentage,
                contract.NativeForestPercentage,
                contract.NonExclusive);

        var kpis = new FarmerKpisForSeasonDto(
            contractDto,
            techRows.Select(k => new TechnologiesKpiRowDto(
                farmerCode,
                season.Id,
                season.Code,
                k.CultureTypeCode,
                k.TechnologyId,
                k.Technology.Name)).ToList(),
            esgIrregularities.Select(k => new EsgIrregularityKpiRowDto(
                farmerCode,
                season.Id,
                season.Code,
                k.CultureTypeCode,
                k.IrregularityTypeId,
                k.IrregularityType.Name)).ToList());

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
