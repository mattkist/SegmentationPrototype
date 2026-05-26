using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class KpiReadService(AppDbContext db) : IKpiReadService
{
    public async Task<IReadOnlyList<FarmerContractKpiRowDto>> ListFarmerContractKpisAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.FarmerContractKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .Select(k => new FarmerContractKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.CultureTypeCode,
                k.DeliveredPercentage,
                k.DeliveredAmountKg,
                k.ContractedAmountKg,
                k.Iqs,
                k.HadNtrm,
                k.HadQualityMixture,
                k.SelfFundingPercentage,
                k.HaveDebt,
                k.Yield,
                k.Scale,
                k.ReforestationPercentage,
                k.NativeForestPercentage,
                k.NonExclusive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.TechnologiesKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .ThenBy(k => k.TechnologyId)
            .Select(k => new TechnologiesKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.CultureTypeCode,
                k.TechnologyId,
                k.Technology.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EsgIrregularityKpiRowDto>> ListEsgIrregularitiesAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.EsgIrregularityKpis.AsNoTracking().Where(k => k.CropSeasonId == cropSeasonId);
        if (cultureTypeCode is not null)
            query = query.Where(k => k.CultureTypeCode == cultureTypeCode);
        return await query
            .OrderBy(k => k.Farmer.Code)
            .ThenBy(k => k.IrregularityTypeId)
            .Select(k => new EsgIrregularityKpiRowDto(
                k.Farmer.Code,
                k.CropSeasonId,
                k.CropSeason.Code,
                k.CultureTypeCode,
                k.IrregularityTypeId,
                k.IrregularityType.Name))
            .ToListAsync(cancellationToken);
    }
}

public sealed class ReferenceDataReadService(AppDbContext db) : IReferenceDataReadService
{
    public async Task<IReadOnlyList<TechnologyDto>> ListTechnologiesAsync(CancellationToken cancellationToken = default) =>
        await db.Technologies.AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new TechnologyDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<IrregularityTypeDto>> ListIrregularityTypesAsync(CancellationToken cancellationToken = default) =>
        await db.IrregularityTypes.AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new IrregularityTypeDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);
}
