using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class SegmentationManagementService(AppDbContext db) : ISegmentationManagementService
{
    public async Task<IReadOnlyList<SegmentationManagementRowDto>> ListAsync(
        int cropSeasonId,
        CancellationToken cancellationToken = default)
    {
        var farmers = await db.Farmers.AsNoTracking().OrderBy(f => f.Code).ToListAsync(cancellationToken);
        var segmentations = await db.FarmerSegmentations.AsNoTracking()
            .Include(s => s.Segment)
            .Where(s => s.CropSeasonId == cropSeasonId)
            .ToDictionaryAsync(s => s.FarmerId, cancellationToken);

        var configIds = segmentations.Values.Select(s => s.SegmentationConfigurationId).Distinct().ToList();
        var configs = await db.SegmentationConfigurations.AsNoTracking()
            .Include(c => c.Segments)
            .Where(c => configIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var contractCulture = await db.FarmerContractKpis.AsNoTracking()
            .Where(k => k.CropSeasonId == cropSeasonId)
            .GroupBy(k => k.FarmerId)
            .Select(g => new { FarmerId = g.Key, CultureTypeCode = g.OrderByDescending(x => x.Scale).Select(x => x.CultureTypeCode).First() })
            .ToDictionaryAsync(x => x.FarmerId, x => x.CultureTypeCode, cancellationToken);

        return farmers.Select(f =>
        {
            segmentations.TryGetValue(f.Id, out var seg);
            var cultureTypeCode = contractCulture.GetValueOrDefault(f.Id) ?? "FCV";

            IReadOnlyList<SegmentationSegmentDto> segments = [];
            if (seg is not null && configs.TryGetValue(seg.SegmentationConfigurationId, out var config))
            {
                segments = config.Segments
                    .OrderBy(s => s.SegmentName)
                    .Select(s => new SegmentationSegmentDto
                    {
                        Id = s.Id,
                        SegmentName = s.SegmentName,
                        OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                        BankDepositDiscount = s.BankDepositDiscount,
                        TobaccoDiscount = s.TobaccoDiscount
                    })
                    .ToList();
            }

            return new SegmentationManagementRowDto
            {
                FarmerId = f.Id,
                FarmerCode = f.Code,
                FarmerName = f.Name,
                CultureTypeCode = cultureTypeCode,
                TotalScore = seg?.TotalScore ?? 0,
                SegmentationConfigurationSegmentId = seg?.SegmentationConfigurationSegmentId,
                SegmentName = seg?.Segment?.SegmentName,
                SegmentationConfigurationId = seg?.SegmentationConfigurationId ?? Guid.Empty,
                AvailableSegments = segments
            };
        }).ToList();
    }

    public async Task SubmitForApprovalAsync(
        Guid farmerId,
        int cropSeasonId,
        SubmitSegmentationApprovalDto dto,
        CancellationToken cancellationToken = default)
    {
        var seg = await db.FarmerSegmentations
            .FirstOrDefaultAsync(s => s.FarmerId == farmerId && s.CropSeasonId == cropSeasonId, cancellationToken);

        if (seg is null)
            throw new KeyNotFoundException($"No official segmentation for farmer '{farmerId}' in crop season '{cropSeasonId}'.");

        if (dto.SegmentationConfigurationSegmentId is { } segmentId)
        {
            var valid = await db.SegmentationSegments.AsNoTracking()
                .AnyAsync(
                    s => s.Id == segmentId && s.SegmentationConfigurationId == seg.SegmentationConfigurationId,
                    cancellationToken);
            if (!valid)
                throw new ArgumentException("Segment is not part of the configuration that produced this official segmentation.");
        }

        seg.SegmentationConfigurationSegmentId = dto.SegmentationConfigurationSegmentId;
        await db.SaveChangesAsync(cancellationToken);
    }
}
