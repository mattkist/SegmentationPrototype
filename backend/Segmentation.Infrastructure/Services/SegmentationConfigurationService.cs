using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Application.Exceptions;
using Segmentation.Domain;
using Segmentation.Domain.Entities;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class SegmentationConfigurationService(AppDbContext db) : ISegmentationConfigurationService
{
    public async Task<IReadOnlyList<SegmentationConfigurationSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await db.SegmentationConfigurations
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SegmentationConfigurationSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                CultureTypeCodes = c.CultureTypes.Select(ct => ct.CultureTypeCode).OrderBy(x => x).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SegmentationConfigurationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LoadFullGraphAsync(id, asNoTracking: true, cancellationToken);
        return entity is null ? null : MapToDetail(entity);
    }

    public async Task<SegmentationConfigurationDetailDto> CreateAsync(
        SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken = default)
    {
        await ValidateDtoAsync(dto, cancellationToken);
        EnsureNonEmptySegments(dto.Segments);

        var id = Guid.NewGuid();
        var entity = MapNewConfiguration(id, dto);
        ValidateAllCultureTypes(entity);

        AssignRelevanceFromMaxScores(entity);
        db.SegmentationConfigurations.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return MapToDetail(entity);
    }

    public async Task<SegmentationConfigurationDetailDto> UpdateAsync(
        Guid id,
        SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken = default)
    {
        await ValidateDtoAsync(dto, cancellationToken);
        EnsureNonEmptySegments(dto.Segments);

        if (!await db.SegmentationConfigurations.AnyAsync(c => c.Id == id, cancellationToken))
            throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await DeleteKpiRangesForConfigurationAsync(id, cancellationToken);
            db.ChangeTracker.Clear();

            var entity = await LoadFullGraphForUpdateAsync(id, cancellationToken);
            if (entity is null)
                throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

            DetachTrackedReplaceableChildren();

            entity.Name = dto.Name;
            await SyncSegmentsAsync(entity, dto.Segments, cancellationToken);
            await SyncCultureTypesAsync(entity, dto.CultureTypes, cancellationToken);

            ValidateAllCultureTypes(entity);
            AssignRelevanceFromMaxScores(entity);

            EnsureKpiRangeEntriesAreAdded();

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            db.ChangeTracker.Clear();
            return (await GetAsync(id, cancellationToken))!;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SegmentationConfigurationDetailDto> PatchNameAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
            throw new ArgumentException("Name is required.", nameof(name));
        if (trimmed.Length > 256)
            throw new ArgumentException("Name must be at most 256 characters.", nameof(name));

        var affected = await db.SegmentationConfigurations
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Name, trimmed), cancellationToken);
        if (affected == 0)
            throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

        return (await GetAsync(id, cancellationToken))!;
    }

    public async Task<SegmentationConfigurationDetailDto> DuplicateAsync(
        Guid id,
        string? name,
        CancellationToken cancellationToken = default)
    {
        var source = await LoadFullGraphAsync(id, asNoTracking: true, cancellationToken);
        if (source is null)
            throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

        var dto = MapToSaveDto(source, name ?? $"{source.Name} (copy)");
        return await CreateAsync(dto, cancellationToken);
    }

    private async Task DeleteKpiRangesForConfigurationAsync(Guid configurationId, CancellationToken cancellationToken)
    {
        var cultureTypeIds = await db.SegmentationConfigurationCultureTypes
            .Where(ct => ct.SegmentationConfigurationId == configurationId)
            .Select(ct => ct.Id)
            .ToListAsync(cancellationToken);

        if (cultureTypeIds.Count > 0)
        {
            await db.SegmentationConfigurationCultureTypeSegments
                .Where(s => cultureTypeIds.Contains(s.SegmentationConfigurationCultureTypeId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        foreach (var cultureTypeId in cultureTypeIds)
        {
            await db.LoyaltySeasonQuantityRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.LoyaltyHistoricalVolumeRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.QualityIqsRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.FinancialSelfFundingRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.YieldRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.ScaleRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
            await db.YieldAndScaleRanges
                .Where(r => r.SegmentationConfigurationCultureTypeId == cultureTypeId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    private async Task<SegmentationConfiguration?> LoadFullGraphForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await db.SegmentationConfigurations
            .Include(c => c.Segments)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Loyalty)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Quality)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Financial)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Technology)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Esg)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Yield)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.Scale)
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.YieldAndScale)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private async Task<SegmentationConfiguration?> LoadFullGraphAsync(
        Guid id,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        IQueryable<SegmentationConfiguration> query = db.SegmentationConfigurations
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
            .Include(c => c.CultureTypes).ThenInclude(ct => ct.YieldAndScale)!.ThenInclude(ys => ys!.Ranges);

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.AsSplitQuery().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    private static void ValidateAllCultureTypes(SegmentationConfiguration entity)
    {
        foreach (var ct in entity.CultureTypes)
        {
            var validation = SegmentationConfigurationKpiMaxScores.ValidateCultureType(ct);
            if (!validation.IsValid)
            {
                throw new SegmentationConfigurationValidationException(
                    validation.ErrorMessage ?? "Invalid configuration.",
                    validation.SumOfKpiMaxScores,
                    ct.MaximumScore);
            }
        }
    }

    private static void AssignRelevanceFromMaxScores(SegmentationConfiguration configuration)
    {
        foreach (var ct in configuration.CultureTypes)
        {
            var m = (decimal)ct.MaximumScore;
            if (m <= 0)
                continue;
            ct.Loyalty!.Relevance = (decimal)ct.Loyalty.MaxScore / m;
            ct.Quality!.Relevance = (decimal)ct.Quality.MaxScore / m;
            ct.Financial!.Relevance = (decimal)ct.Financial.MaxScore / m;
            ct.Technology!.Relevance = (decimal)ct.Technology.MaxScore / m;
            ct.Esg!.Relevance = (decimal)ct.Esg.MaxScore / m;
            ct.Yield!.Relevance = (decimal)ct.Yield.MaxScore / m;
            ct.Scale!.Relevance = (decimal)ct.Scale.MaxScore / m;
            ct.YieldAndScale!.Relevance = (decimal)ct.YieldAndScale.MaxScore / m;
        }
    }

    private async Task ValidateDtoAsync(SaveSegmentationConfigurationDto dto, CancellationToken cancellationToken)
    {
        if (dto.CultureTypes.Count == 0)
            throw new ArgumentException("At least one culture type configuration is required.");

        var codes = dto.CultureTypes.Select(c => c.CultureTypeCode).ToList();
        if (codes.Distinct(StringComparer.Ordinal).Count() != codes.Count)
            throw new ArgumentException("Duplicate culture type codes are not allowed.");

        foreach (var code in codes)
        {
            if (!await db.CultureTypes.AnyAsync(c => c.Code == code, cancellationToken))
                throw new ArgumentException($"Unknown culture type code '{code}'.");
        }
    }

    private static void EnsureNonEmptySegments(IReadOnlyList<SegmentationSegmentDto> segments)
    {
        if (segments.Count == 0)
            throw new ArgumentException("At least one segment is required.");
    }

    private async Task SyncSegmentsAsync(
        SegmentationConfiguration entity,
        IReadOnlyList<SegmentationSegmentDto> dtoSegments,
        CancellationToken cancellationToken)
    {
        var existing = entity.Segments.ToDictionary(s => s.Id);
        var keepIds = new HashSet<Guid>();

        foreach (var dto in dtoSegments)
        {
            if (dto.Id is { } sid && existing.TryGetValue(sid, out var seg))
            {
                seg.SegmentName = dto.SegmentName;
                seg.OnlyExclusiveFarmer = dto.OnlyExclusiveFarmer;
                seg.BankDepositDiscount = dto.BankDepositDiscount;
                seg.TobaccoDiscount = dto.TobaccoDiscount;
                keepIds.Add(sid);
            }
            else
            {
                var newId = dto.Id ?? Guid.NewGuid();
                entity.Segments.Add(new SegmentationSegment
                {
                    Id = newId,
                    SegmentationConfigurationId = entity.Id,
                    SegmentName = dto.SegmentName,
                    OnlyExclusiveFarmer = dto.OnlyExclusiveFarmer,
                    BankDepositDiscount = dto.BankDepositDiscount,
                    TobaccoDiscount = dto.TobaccoDiscount
                });
                keepIds.Add(newId);
            }
        }

        var toRemove = entity.Segments.Where(s => !keepIds.Contains(s.Id)).ToList();
        foreach (var seg in toRemove)
            entity.Segments.Remove(seg);
    }

    private async Task SyncCultureTypesAsync(
        SegmentationConfiguration entity,
        IReadOnlyList<CultureTypeConfigurationWriteDto> dtoCultureTypes,
        CancellationToken cancellationToken)
    {
        var existing = entity.CultureTypes.ToDictionary(ct => ct.CultureTypeCode);
        var keepCodes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var dto in dtoCultureTypes)
        {
            keepCodes.Add(dto.CultureTypeCode);
            SegmentationConfigurationCultureType ct;
            if (existing.TryGetValue(dto.CultureTypeCode, out var found))
            {
                ct = found;
                ct.MaximumScore = dto.MaximumScore;
            }
            else
            {
                ct = new SegmentationConfigurationCultureType
                {
                    Id = dto.Id ?? Guid.NewGuid(),
                    SegmentationConfigurationId = entity.Id,
                    CultureTypeCode = dto.CultureTypeCode,
                    MaximumScore = dto.MaximumScore
                };
                entity.CultureTypes.Add(ct);
            }

            SyncCultureTypeSegmentsForUpdate(ct, dto.SegmentThresholds, entity.Segments);
            ApplyKpiBlocksForUpdate(ct, dto);
        }

        foreach (var removed in entity.CultureTypes.Where(ct => !keepCodes.Contains(ct.CultureTypeCode)).ToList())
            entity.CultureTypes.Remove(removed);
    }

    private static void SyncCultureTypeSegments(
        SegmentationConfigurationCultureType ct,
        IReadOnlyList<CultureTypeSegmentThresholdDto> thresholds,
        ICollection<SegmentationSegment> headerSegments)
    {
        ct.CultureTypeSegments = BuildCultureTypeSegments(ct.Id, thresholds, headerSegments);
    }

    private void SyncCultureTypeSegmentsForUpdate(
        SegmentationConfigurationCultureType ct,
        IReadOnlyList<CultureTypeSegmentThresholdDto> thresholds,
        ICollection<SegmentationSegment> headerSegments)
    {
        var segments = BuildCultureTypeSegments(ct.Id, thresholds, headerSegments);
        ct.CultureTypeSegments = segments;
        db.SegmentationConfigurationCultureTypeSegments.AddRange(segments);
    }

    private static List<SegmentationConfigurationCultureTypeSegment> BuildCultureTypeSegments(
        Guid cultureTypeId,
        IReadOnlyList<CultureTypeSegmentThresholdDto> thresholds,
        ICollection<SegmentationSegment> headerSegments)
    {
        var segmentByName = headerSegments.ToDictionary(s => s.SegmentName, StringComparer.OrdinalIgnoreCase);
        var list = new List<SegmentationConfigurationCultureTypeSegment>();

        foreach (var t in thresholds)
        {
            var segment = t.SegmentId is { } sid
                ? headerSegments.FirstOrDefault(s => s.Id == sid)
                : segmentByName.GetValueOrDefault(t.SegmentName);

            if (segment is null)
                throw new ArgumentException($"Segment '{t.SegmentName}' was not found on the configuration header.");

            list.Add(new SegmentationConfigurationCultureTypeSegment
            {
                Id = Guid.NewGuid(),
                SegmentationConfigurationCultureTypeId = cultureTypeId,
                SegmentationSegmentId = segment.Id,
                RangeMin = t.RangeMin
            });
        }

        return list;
    }

    private void DetachTrackedReplaceableChildren()
    {
        DetachAllTracked<SegmentationConfigurationCultureTypeSegment>();
        DetachAllTracked<LoyaltySeasonQuantityRange>();
        DetachAllTracked<LoyaltyHistoricalVolumeRange>();
        DetachAllTracked<QualityIqsRange>();
        DetachAllTracked<FinancialSelfFundingRange>();
        DetachAllTracked<YieldRange>();
        DetachAllTracked<ScaleRange>();
        DetachAllTracked<YieldAndScaleRange>();
    }

    private void DetachAllTracked<T>() where T : class
    {
        foreach (var entry in db.ChangeTracker.Entries<T>())
            entry.State = EntityState.Detached;
    }

    /// <summary>
    /// After bulk-delete + re-insert, range rows must never be Updated (only Added).
    /// </summary>
    private void EnsureKpiRangeEntriesAreAdded()
    {
        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.Entity is not (
                LoyaltySeasonQuantityRange or
                LoyaltyHistoricalVolumeRange or
                QualityIqsRange or
                FinancialSelfFundingRange or
                YieldRange or
                ScaleRange or
                YieldAndScaleRange))
                continue;

            if (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged)
                entry.State = EntityState.Added;
        }

        foreach (var entry in db.ChangeTracker.Entries<SegmentationConfigurationCultureTypeSegment>())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Unchanged)
                entry.State = EntityState.Added;
        }
    }

    /// <summary>
    /// Replaces KPI range rows on update. Ranges are bulk-deleted before load; only inserts run on save.
    /// </summary>
    private void ApplyKpiBlocksForUpdate(SegmentationConfigurationCultureType ct, CultureTypeConfigurationWriteDto dto)
    {
        var cultureTypeId = ct.Id;

        ct.Loyalty ??= new SegmentationConfigurationLoyalty { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Loyalty.Relevance = dto.Loyalty.Relevance;
        var seasonRanges = dto.Loyalty.SeasonQuantityRanges.Select(r => new LoyaltySeasonQuantityRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
            MinimumDeliveryAmount = r.MinimumDeliveryAmount,
            MaximumDeliveryAmount = r.MaximumDeliveryAmount,
            DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
            Score = r.Score
        }).ToList();
        ct.Loyalty.SeasonQuantityRanges = seasonRanges;
        db.LoyaltySeasonQuantityRanges.AddRange(seasonRanges);

        var historicalRanges = dto.Loyalty.HistoricalVolumeRanges.Select(h => new LoyaltyHistoricalVolumeRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            MinimumDeliveryAmount = h.MinimumDeliveryAmount,
            MaximumDeliveryAmount = h.MaximumDeliveryAmount,
            Score = h.Score
        }).ToList();
        ct.Loyalty.HistoricalVolumeRanges = historicalRanges;
        db.LoyaltyHistoricalVolumeRanges.AddRange(historicalRanges);

        ct.Quality ??= new SegmentationConfigurationQuality { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Quality.Relevance = dto.Quality.Relevance;
        ct.Quality.NtrmScore = dto.Quality.NtrmScore;
        ct.Quality.MixtureScore = dto.Quality.MixtureScore;
        var iqsRanges = dto.Quality.IqsRanges.Select(r => new QualityIqsRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();
        ct.Quality.IqsRanges = iqsRanges;
        db.QualityIqsRanges.AddRange(iqsRanges);

        ct.Financial ??= new SegmentationConfigurationFinancial { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Financial.Relevance = dto.Financial.Relevance;
        ct.Financial.DebtScore = dto.Financial.DebtScore;
        var selfFundingRanges = dto.Financial.SelfFundingRanges.Select(r => new FinancialSelfFundingRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();
        ct.Financial.SelfFundingRanges = selfFundingRanges;
        db.FinancialSelfFundingRanges.AddRange(selfFundingRanges);

        ct.Technology ??= new SegmentationConfigurationTechnology { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Technology.Relevance = dto.Technology.Relevance;
        ct.Technology.HasLargeBaseRidgeWithMulchScore = dto.Technology.HasLargeBaseRidgeWithMulchScore;
        ct.Technology.HasBroadGrateFurnaceScore = dto.Technology.HasBroadGrateFurnaceScore;
        ct.Technology.HasTechnologyPackageAdherenceScore = dto.Technology.HasTechnologyPackageAdherenceScore;
        ct.Technology.HasStandardBarnScore = dto.Technology.HasStandardBarnScore;

        ct.Esg ??= new SegmentationConfigurationEsg { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Esg.Relevance = dto.Esg.Relevance;
        ct.Esg.ReforestationScorePerPercentualPoint = dto.Esg.ReforestationScorePerPercentualPoint;
        ct.Esg.ReforestationMaximumScore = dto.Esg.ReforestationMaximumScore;
        ct.Esg.NativeForestScorePerPercentualPoint = dto.Esg.NativeForestScorePerPercentualPoint;
        ct.Esg.NativeForestMaximumScore = dto.Esg.NativeForestMaximumScore;
        ct.Esg.MinorIrregularityScore = dto.Esg.MinorIrregularityScore;
        ct.Esg.MajorIrregularityScore = dto.Esg.MajorIrregularityScore;

        ct.Yield ??= new SegmentationConfigurationYield { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Yield.Relevance = dto.Yield.Relevance;
        var yieldRanges = dto.Yield.Ranges.Select(r => new YieldRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();
        ct.Yield.Ranges = yieldRanges;
        db.YieldRanges.AddRange(yieldRanges);

        ct.Scale ??= new SegmentationConfigurationScale { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.Scale.Relevance = dto.Scale.Relevance;
        var scaleRanges = dto.Scale.Ranges.Select(r => new ScaleRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();
        ct.Scale.Ranges = scaleRanges;
        db.ScaleRanges.AddRange(scaleRanges);

        ct.YieldAndScale ??= new SegmentationConfigurationYieldAndScale { SegmentationConfigurationCultureTypeId = cultureTypeId };
        ct.YieldAndScale.Relevance = dto.YieldAndScale.Relevance;
        var yieldAndScaleRanges = dto.YieldAndScale.Ranges.Select(r => new YieldAndScaleRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = cultureTypeId,
            YieldAndScaleCropSeasonAmount = r.YieldAndScaleCropSeasonAmount,
            MinimumYield = r.MinimumYield,
            MaximumYield = r.MaximumYield,
            MinimumModule = r.MinimumModule,
            MaximumModule = r.MaximumModule,
            Score = r.Score
        }).ToList();
        ct.YieldAndScale.Ranges = yieldAndScaleRanges;
        db.YieldAndScaleRanges.AddRange(yieldAndScaleRanges);
    }

    private static void ApplyKpiBlocks(SegmentationConfigurationCultureType ct, CultureTypeConfigurationWriteDto dto)
    {
        ct.Loyalty ??= new SegmentationConfigurationLoyalty { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Loyalty.Relevance = dto.Loyalty.Relevance;
        ct.Loyalty.SeasonQuantityRanges = dto.Loyalty.SeasonQuantityRanges.Select(r => new LoyaltySeasonQuantityRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
            MinimumDeliveryAmount = r.MinimumDeliveryAmount,
            MaximumDeliveryAmount = r.MaximumDeliveryAmount,
            DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
            Score = r.Score
        }).ToList();

        ct.Loyalty.HistoricalVolumeRanges = dto.Loyalty.HistoricalVolumeRanges.Select(h => new LoyaltyHistoricalVolumeRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            MinimumDeliveryAmount = h.MinimumDeliveryAmount,
            MaximumDeliveryAmount = h.MaximumDeliveryAmount,
            Score = h.Score
        }).ToList();

        ct.Quality ??= new SegmentationConfigurationQuality { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Quality.Relevance = dto.Quality.Relevance;
        ct.Quality.NtrmScore = dto.Quality.NtrmScore;
        ct.Quality.MixtureScore = dto.Quality.MixtureScore;
        ct.Quality.IqsRanges = dto.Quality.IqsRanges.Select(r => new QualityIqsRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();

        ct.Financial ??= new SegmentationConfigurationFinancial { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Financial.Relevance = dto.Financial.Relevance;
        ct.Financial.DebtScore = dto.Financial.DebtScore;
        ct.Financial.SelfFundingRanges = dto.Financial.SelfFundingRanges.Select(r => new FinancialSelfFundingRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();

        ct.Technology ??= new SegmentationConfigurationTechnology { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Technology.Relevance = dto.Technology.Relevance;
        ct.Technology.HasLargeBaseRidgeWithMulchScore = dto.Technology.HasLargeBaseRidgeWithMulchScore;
        ct.Technology.HasBroadGrateFurnaceScore = dto.Technology.HasBroadGrateFurnaceScore;
        ct.Technology.HasTechnologyPackageAdherenceScore = dto.Technology.HasTechnologyPackageAdherenceScore;
        ct.Technology.HasStandardBarnScore = dto.Technology.HasStandardBarnScore;

        ct.Esg ??= new SegmentationConfigurationEsg { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Esg.Relevance = dto.Esg.Relevance;
        ct.Esg.ReforestationScorePerPercentualPoint = dto.Esg.ReforestationScorePerPercentualPoint;
        ct.Esg.ReforestationMaximumScore = dto.Esg.ReforestationMaximumScore;
        ct.Esg.NativeForestScorePerPercentualPoint = dto.Esg.NativeForestScorePerPercentualPoint;
        ct.Esg.NativeForestMaximumScore = dto.Esg.NativeForestMaximumScore;
        ct.Esg.MinorIrregularityScore = dto.Esg.MinorIrregularityScore;
        ct.Esg.MajorIrregularityScore = dto.Esg.MajorIrregularityScore;

        ct.Yield ??= new SegmentationConfigurationYield { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Yield.Relevance = dto.Yield.Relevance;
        ct.Yield.Ranges = dto.Yield.Ranges.Select(r => new YieldRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();

        ct.Scale ??= new SegmentationConfigurationScale { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.Scale.Relevance = dto.Scale.Relevance;
        ct.Scale.Ranges = dto.Scale.Ranges.Select(r => new ScaleRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList();

        ct.YieldAndScale ??= new SegmentationConfigurationYieldAndScale { SegmentationConfigurationCultureTypeId = ct.Id };
        ct.YieldAndScale.Relevance = dto.YieldAndScale.Relevance;
        ct.YieldAndScale.Ranges = dto.YieldAndScale.Ranges.Select(r => new YieldAndScaleRange
        {
            Id = Guid.NewGuid(),
            SegmentationConfigurationCultureTypeId = ct.Id,
            YieldAndScaleCropSeasonAmount = r.YieldAndScaleCropSeasonAmount,
            MinimumYield = r.MinimumYield,
            MaximumYield = r.MaximumYield,
            MinimumModule = r.MinimumModule,
            MaximumModule = r.MaximumModule,
            Score = r.Score
        }).ToList();
    }

    private static SegmentationConfiguration MapNewConfiguration(Guid id, SaveSegmentationConfigurationDto dto)
    {
        var entity = new SegmentationConfiguration
        {
            Id = id,
            Name = dto.Name,
            Segments = dto.Segments.Select(s => new SegmentationSegment
            {
                Id = s.Id ?? Guid.NewGuid(),
                SegmentationConfigurationId = id,
                SegmentName = s.SegmentName,
                OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                BankDepositDiscount = s.BankDepositDiscount,
                TobaccoDiscount = s.TobaccoDiscount
            }).ToList()
        };

        foreach (var ctDto in dto.CultureTypes)
        {
            var ctId = ctDto.Id ?? Guid.NewGuid();
            var ct = new SegmentationConfigurationCultureType
            {
                Id = ctId,
                SegmentationConfigurationId = id,
                CultureTypeCode = ctDto.CultureTypeCode,
                MaximumScore = ctDto.MaximumScore
            };
            entity.CultureTypes.Add(ct);
            SyncCultureTypeSegments(ct, ctDto.SegmentThresholds, entity.Segments);
            ApplyKpiBlocks(ct, ctDto);
        }

        return entity;
    }

    private static SaveSegmentationConfigurationDto MapToSaveDto(SegmentationConfiguration c, string? nameOverride = null)
    {
        SegmentationConfigurationKpiMaxScores.SynchronizeAll(c);

        return new SaveSegmentationConfigurationDto
        {
            Name = nameOverride ?? c.Name,
            Segments = c.Segments.Select(s => new SegmentationSegmentDto
            {
                Id = s.Id,
                SegmentName = s.SegmentName,
                OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                BankDepositDiscount = s.BankDepositDiscount,
                TobaccoDiscount = s.TobaccoDiscount
            }).ToList(),
            CultureTypes = c.CultureTypes.OrderBy(ct => ct.CultureTypeCode).Select(MapCultureTypeWrite).ToList()
        };
    }

    private static CultureTypeConfigurationWriteDto MapCultureTypeWrite(SegmentationConfigurationCultureType ct) => new()
    {
        Id = ct.Id,
        CultureTypeCode = ct.CultureTypeCode,
        MaximumScore = ct.MaximumScore,
        SegmentThresholds = ct.CultureTypeSegments
            .Select(cs => new CultureTypeSegmentThresholdDto
            {
                SegmentId = cs.SegmentationSegmentId,
                SegmentName = cs.Segment.SegmentName,
                RangeMin = cs.RangeMin
            }).ToList(),
        Loyalty = MapLoyaltyWrite(ct.Loyalty!),
        Quality = MapQualityWrite(ct.Quality!),
        Financial = MapFinancialWrite(ct.Financial!),
        Technology = MapTechnologyWrite(ct.Technology!),
        Esg = MapEsgWrite(ct.Esg!),
        Yield = MapYieldWrite(ct.Yield!),
        Scale = MapScaleWrite(ct.Scale!),
        YieldAndScale = MapYieldAndScaleWrite(ct.YieldAndScale!)
    };

    private static SegmentationLoyaltyWriteDto MapLoyaltyWrite(SegmentationConfigurationLoyalty l) => new()
    {
        Relevance = l.Relevance,
        SeasonQuantityRanges = l.SeasonQuantityRanges.Select(r => new LoyaltySeasonQuantityRangeDto
        {
            PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
            MinimumDeliveryAmount = r.MinimumDeliveryAmount,
            MaximumDeliveryAmount = r.MaximumDeliveryAmount,
            DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
            Score = r.Score
        }).ToList(),
        HistoricalVolumeRanges = l.HistoricalVolumeRanges.Select(h => new LoyaltyHistoricalVolumeRangeDto
        {
            MinimumDeliveryAmount = h.MinimumDeliveryAmount,
            MaximumDeliveryAmount = h.MaximumDeliveryAmount,
            Score = h.Score
        }).ToList()
    };

    private static SegmentationQualityWriteDto MapQualityWrite(SegmentationConfigurationQuality q) => new()
    {
        Relevance = q.Relevance,
        NtrmScore = q.NtrmScore,
        MixtureScore = q.MixtureScore,
        IqsRanges = q.IqsRanges.Select(r => new QualityIqsRangeDto
        {
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList()
    };

    private static SegmentationFinancialWriteDto MapFinancialWrite(SegmentationConfigurationFinancial f) => new()
    {
        Relevance = f.Relevance,
        DebtScore = f.DebtScore,
        SelfFundingRanges = f.SelfFundingRanges.Select(r => new FinancialSelfFundingRangeDto
        {
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList()
    };

    private static SegmentationTechnologyWriteDto MapTechnologyWrite(SegmentationConfigurationTechnology t) => new()
    {
        Relevance = t.Relevance,
        HasLargeBaseRidgeWithMulchScore = t.HasLargeBaseRidgeWithMulchScore,
        HasBroadGrateFurnaceScore = t.HasBroadGrateFurnaceScore,
        HasTechnologyPackageAdherenceScore = t.HasTechnologyPackageAdherenceScore,
        HasStandardBarnScore = t.HasStandardBarnScore
    };

    private static SegmentationEsgWriteDto MapEsgWrite(SegmentationConfigurationEsg e) => new()
    {
        Relevance = e.Relevance,
        ReforestationScorePerPercentualPoint = e.ReforestationScorePerPercentualPoint,
        ReforestationMaximumScore = e.ReforestationMaximumScore,
        NativeForestScorePerPercentualPoint = e.NativeForestScorePerPercentualPoint,
        NativeForestMaximumScore = e.NativeForestMaximumScore,
        MinorIrregularityScore = e.MinorIrregularityScore,
        MajorIrregularityScore = e.MajorIrregularityScore
    };

    private static SegmentationYieldWriteDto MapYieldWrite(SegmentationConfigurationYield y) => new()
    {
        Relevance = y.Relevance,
        Ranges = y.Ranges.Select(r => new YieldRangeDto
        {
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList()
    };

    private static SegmentationScaleWriteDto MapScaleWrite(SegmentationConfigurationScale s) => new()
    {
        Relevance = s.Relevance,
        Ranges = s.Ranges.Select(r => new ScaleRangeDto
        {
            Minimum = r.Minimum,
            Maximum = r.Maximum,
            CropSeasonAmount = r.CropSeasonAmount,
            Score = r.Score
        }).ToList()
    };

    private static SegmentationYieldAndScaleWriteDto MapYieldAndScaleWrite(SegmentationConfigurationYieldAndScale ys) => new()
    {
        Relevance = ys.Relevance,
        Ranges = ys.Ranges.Select(r => new YieldAndScaleRangeDto
        {
            YieldAndScaleCropSeasonAmount = r.YieldAndScaleCropSeasonAmount,
            MinimumYield = r.MinimumYield,
            MaximumYield = r.MaximumYield,
            MinimumModule = r.MinimumModule,
            MaximumModule = r.MaximumModule,
            Score = r.Score
        }).ToList()
    };

    private static SegmentationConfigurationDetailDto MapToDetail(SegmentationConfiguration c)
    {
        SegmentationConfigurationKpiMaxScores.SynchronizeAll(c);

        return new SegmentationConfigurationDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            Segments = c.Segments
                .OrderBy(s => s.SegmentName)
                .Select(s => new SegmentationSegmentDto
                {
                    Id = s.Id,
                    SegmentName = s.SegmentName,
                    OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                    BankDepositDiscount = s.BankDepositDiscount,
                    TobaccoDiscount = s.TobaccoDiscount
                }).ToList(),
            CultureTypes = c.CultureTypes.OrderBy(ct => ct.CultureTypeCode).Select(ct => new CultureTypeConfigurationDetailDto
            {
                Id = ct.Id,
                CultureTypeCode = ct.CultureTypeCode,
                MaximumScore = ct.MaximumScore,
                SegmentThresholds = ct.CultureTypeSegments
                    .Select(cs => new CultureTypeSegmentThresholdDto
                    {
                        SegmentId = cs.SegmentationSegmentId,
                        SegmentName = cs.Segment.SegmentName,
                        RangeMin = cs.RangeMin
                    }).ToList(),
                Loyalty = new SegmentationLoyaltyDetailDto
                {
                    MaxScore = ct.Loyalty!.MaxScore,
                    Relevance = ct.Loyalty.Relevance,
                    SeasonQuantityRanges = ct.Loyalty.SeasonQuantityRanges.Select(r => new LoyaltySeasonQuantityRangeDto
                    {
                        PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
                        MinimumDeliveryAmount = r.MinimumDeliveryAmount,
                        MaximumDeliveryAmount = r.MaximumDeliveryAmount,
                        DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
                        Score = r.Score
                    }).ToList(),
                    HistoricalVolumeRanges = ct.Loyalty.HistoricalVolumeRanges.Select(h => new LoyaltyHistoricalVolumeRangeDto
                    {
                        MinimumDeliveryAmount = h.MinimumDeliveryAmount,
                        MaximumDeliveryAmount = h.MaximumDeliveryAmount,
                        Score = h.Score
                    }).ToList()
                },
                Quality = new SegmentationQualityDetailDto
                {
                    MaxScore = ct.Quality!.MaxScore,
                    Relevance = ct.Quality.Relevance,
                    NtrmScore = ct.Quality.NtrmScore,
                    MixtureScore = ct.Quality.MixtureScore,
                    IqsRanges = ct.Quality.IqsRanges.Select(r => new QualityIqsRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        Score = r.Score
                    }).ToList()
                },
                Financial = new SegmentationFinancialDetailDto
                {
                    MaxScore = ct.Financial!.MaxScore,
                    Relevance = ct.Financial.Relevance,
                    DebtScore = ct.Financial.DebtScore,
                    SelfFundingRanges = ct.Financial.SelfFundingRanges.Select(r => new FinancialSelfFundingRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        Score = r.Score
                    }).ToList()
                },
                Technology = new SegmentationTechnologyDetailDto
                {
                    MaxScore = ct.Technology!.MaxScore,
                    Relevance = ct.Technology.Relevance,
                    HasLargeBaseRidgeWithMulchScore = ct.Technology.HasLargeBaseRidgeWithMulchScore,
                    HasBroadGrateFurnaceScore = ct.Technology.HasBroadGrateFurnaceScore,
                    HasTechnologyPackageAdherenceScore = ct.Technology.HasTechnologyPackageAdherenceScore,
                    HasStandardBarnScore = ct.Technology.HasStandardBarnScore
                },
                Esg = new SegmentationEsgDetailDto
                {
                    MaxScore = ct.Esg!.MaxScore,
                    Relevance = ct.Esg.Relevance,
                    ReforestationScorePerPercentualPoint = ct.Esg.ReforestationScorePerPercentualPoint,
                    ReforestationMaximumScore = ct.Esg.ReforestationMaximumScore,
                    NativeForestScorePerPercentualPoint = ct.Esg.NativeForestScorePerPercentualPoint,
                    NativeForestMaximumScore = ct.Esg.NativeForestMaximumScore,
                    MinorIrregularityScore = ct.Esg.MinorIrregularityScore,
                    MajorIrregularityScore = ct.Esg.MajorIrregularityScore
                },
                Yield = new SegmentationYieldDetailDto
                {
                    MaxScore = ct.Yield!.MaxScore,
                    Relevance = ct.Yield.Relevance,
                    Ranges = ct.Yield.Ranges.Select(r => new YieldRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        Score = r.Score
                    }).ToList()
                },
                Scale = new SegmentationScaleDetailDto
                {
                    MaxScore = ct.Scale!.MaxScore,
                    Relevance = ct.Scale.Relevance,
                    Ranges = ct.Scale.Ranges.Select(r => new ScaleRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        Score = r.Score
                    }).ToList()
                },
                YieldAndScale = new SegmentationYieldAndScaleDetailDto
                {
                    MaxScore = ct.YieldAndScale!.MaxScore,
                    Relevance = ct.YieldAndScale.Relevance,
                    Ranges = ct.YieldAndScale.Ranges.Select(r => new YieldAndScaleRangeDto
                    {
                        YieldAndScaleCropSeasonAmount = r.YieldAndScaleCropSeasonAmount,
                        MinimumYield = r.MinimumYield,
                        MaximumYield = r.MaximumYield,
                        MinimumModule = r.MinimumModule,
                        MaximumModule = r.MaximumModule,
                        Score = r.Score
                    }).ToList()
                }
            }).ToList()
        };
    }
}
