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
                MaximumScore = c.MaximumScore
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
        await ValidateCropSeasonReferencesAsync(dto, cancellationToken);
        EnsureNonEmptySegments(dto.Segments);

        var id = Guid.NewGuid();
        var entity = MapNewConfiguration(id, dto);
        var validation = SegmentationConfigurationKpiMaxScores.ValidateAgainstMaximum(entity);
        if (!validation.IsValid)
        {
            throw new SegmentationConfigurationValidationException(
                validation.ErrorMessage ?? "Invalid configuration.",
                validation.SumOfKpiMaxScores,
                entity.MaximumScore);
        }

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
        await ValidateCropSeasonReferencesAsync(dto, cancellationToken);
        EnsureNonEmptySegments(dto.Segments);

        if (!await db.SegmentationConfigurations.AnyAsync(c => c.Id == id, cancellationToken))
            throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Linear SQLite path: delete KPI range rows with parameterized SQL only (EF change tracker not used for
            // deletes), clear tracker, load slim graph once, then a single SaveChanges for inserts + updates.
            await DeleteAllKpiRangeRowsForConfigurationSqlAsync(id, cancellationToken);
            db.ChangeTracker.Clear();

            var entity = await LoadFullGraphAsync(id, asNoTracking: false, cancellationToken, includeVariableLengthRows: false);
            if (entity is null)
                throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

            entity.Name = dto.Name;
            entity.MaximumScore = dto.MaximumScore;

            await SyncSegmentsAsync(entity, dto.Segments, cancellationToken);

            ReplaceLoyaltyRanges(entity.Loyalty!, dto.Loyalty);
            ReplaceQualityRanges(entity.Quality!, dto.Quality);
            ReplaceFinancialRanges(entity.Financial!, dto.Financial);
            ReplaceYieldRanges(entity.Yield!, dto.Yield);
            ReplaceScaleRanges(entity.Scale!, dto.Scale);

            ApplyLoyaltyScalars(entity.Loyalty!, dto.Loyalty);
            ApplyQualityScalars(entity.Quality!, dto.Quality);
            ApplyFinancialScalars(entity.Financial!, dto.Financial);
            ApplyTechnologyScalars(entity.Technology!, dto.Technology);
            ApplyEsgScalars(entity.Esg!, dto.Esg);
            ApplyYieldScalars(entity.Yield!, dto.Yield);
            ApplyScaleScalars(entity.Scale!, dto.Scale);

            var validation = SegmentationConfigurationKpiMaxScores.ValidateAgainstMaximum(entity);
            if (!validation.IsValid)
            {
                throw new SegmentationConfigurationValidationException(
                    validation.ErrorMessage ?? "Invalid configuration.",
                    validation.SumOfKpiMaxScores,
                    entity.MaximumScore);
            }

            AssignRelevanceFromMaxScores(entity);

            NormalizeKpiRangeRowStatesAfterSqlDeletes();

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return MapToDetail(entity);
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

    /// <summary>
    /// Deletes all variable-length KPI range rows for a configuration using sequential SQL (SQLite).
    /// Junction rows first, then range tables. Does not use the EF change tracker, so it cannot conflict with
    /// subsequent inserts in the same transaction.
    /// </summary>
    private async Task DeleteAllKpiRangeRowsForConfigurationSqlAsync(Guid configurationId, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""LoyaltySeasonQuantityRangeSkippedCropSeasons"" WHERE ""LoyaltySeasonQuantityRangeId"" IN (
                SELECT ""Id"" FROM ""LoyaltySeasonQuantityRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId})",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""LoyaltySeasonQuantityRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""LoyaltyHistoricalVolumeRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""QualityIqsRangeSkippedCropSeasons"" WHERE ""QualityIqsRangeId"" IN (
                SELECT ""Id"" FROM ""QualityIqsRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId})",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""QualityIqsRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""FinancialSelfFundingRangeSkippedCropSeasons"" WHERE ""FinancialSelfFundingRangeId"" IN (
                SELECT ""Id"" FROM ""FinancialSelfFundingRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId})",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""FinancialSelfFundingRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""YieldRangeSkippedCropSeasons"" WHERE ""YieldRangeId"" IN (
                SELECT ""Id"" FROM ""YieldRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId})",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""YieldRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);

        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""ScaleRangeSkippedCropSeasons"" WHERE ""ScaleRangeId"" IN (
                SELECT ""Id"" FROM ""ScaleRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId})",
            cancellationToken);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $@"DELETE FROM ""ScaleRanges"" WHERE ""SegmentationConfigurationId"" = {configurationId}",
            cancellationToken);
    }

    public async Task<SegmentationConfigurationDetailDto> DuplicateAsync(
        Guid id,
        string? newName,
        CancellationToken cancellationToken = default)
    {
        var source = await LoadFullGraphAsync(id, asNoTracking: true, cancellationToken);
        if (source is null)
            throw new KeyNotFoundException($"Segmentation configuration '{id}' was not found.");

        var cloneId = Guid.NewGuid();
        var cloneName = newName ?? $"{source.Name} (Copy)";
        var clone = CloneConfiguration(source, cloneId, cloneName);

        var validation = SegmentationConfigurationKpiMaxScores.ValidateAgainstMaximum(clone);
        if (!validation.IsValid)
        {
            throw new SegmentationConfigurationValidationException(
                validation.ErrorMessage ?? "Invalid configuration.",
                validation.SumOfKpiMaxScores,
                clone.MaximumScore);
        }

        db.SegmentationConfigurations.Add(clone);
        await db.SaveChangesAsync(cancellationToken);
        return MapToDetail(clone);
    }

    private static void EnsureNonEmptySegments(IReadOnlyList<SegmentationSegmentDto> segments)
    {
        if (segments.Count == 0)
            throw new ArgumentException("At least one segment is required.", nameof(segments));
    }

    private async Task ValidateCropSeasonReferencesAsync(
        SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken)
    {
        var ids = CollectCropSeasonIds(dto).Distinct().ToList();
        if (ids.Count == 0)
            return;

        var existing = await db.CropSeasons.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missing = ids.Except(existing).ToList();
        if (missing.Count > 0)
        {
            throw new SegmentationConfigurationValidationException(
                $"Unknown crop season id(s): {string.Join(", ", missing)}.",
                0,
                dto.MaximumScore);
        }
    }

    private static HashSet<int> CollectCropSeasonIds(SaveSegmentationConfigurationDto dto)
    {
        var set = new HashSet<int>();
        foreach (var r in dto.Loyalty.SeasonQuantityRanges)
            foreach (var s in r.SkippedCropSeasonIds)
                set.Add(s);
        foreach (var r in dto.Quality.IqsRanges)
            foreach (var s in r.SkippedCropSeasonIds)
                set.Add(s);
        foreach (var r in dto.Financial.SelfFundingRanges)
            foreach (var s in r.SkippedCropSeasonIds)
                set.Add(s);
        foreach (var r in dto.Yield.Ranges)
            foreach (var s in r.SkippedCropSeasonIds)
                set.Add(s);
        foreach (var r in dto.Scale.Ranges)
            foreach (var s in r.SkippedCropSeasonIds)
                set.Add(s);
        return set;
    }

    /// <param name="includeVariableLengthRows">
    /// When false, skips <c>ThenInclude</c> of KPI range collections (smaller read for callers that do not need rules).
    /// </param>
    private async Task<SegmentationConfiguration?> LoadFullGraphAsync(
        Guid id,
        bool asNoTracking,
        CancellationToken cancellationToken,
        bool includeVariableLengthRows = true)
    {
        IQueryable<SegmentationConfiguration> query = db.SegmentationConfigurations
            .Include(c => c.Segments)
            .Where(c => c.Id == id);

        if (includeVariableLengthRows)
        {
            query = query
                .Include(c => c.Loyalty)!.ThenInclude(l => l!.SeasonQuantityRanges).ThenInclude(r => r.SkippedCropSeasons)
                .Include(c => c.Loyalty)!.ThenInclude(l => l!.HistoricalVolumeRanges)
                .Include(c => c.Quality)!.ThenInclude(q => q!.IqsRanges).ThenInclude(r => r.SkippedCropSeasons)
                .Include(c => c.Financial)!.ThenInclude(f => f!.SelfFundingRanges).ThenInclude(r => r.SkippedCropSeasons)
                .Include(c => c.Technology)
                .Include(c => c.Esg)
                .Include(c => c.Yield)!.ThenInclude(y => y!.Ranges).ThenInclude(r => r.SkippedCropSeasons)
                .Include(c => c.Scale)!.ThenInclude(s => s!.Ranges).ThenInclude(r => r.SkippedCropSeasons);
        }
        else
        {
            query = query
                .Include(c => c.Loyalty)
                .Include(c => c.Quality)
                .Include(c => c.Financial)
                .Include(c => c.Technology)
                .Include(c => c.Esg)
                .Include(c => c.Yield)
                .Include(c => c.Scale);
        }

        if (asNoTracking)
            query = query.AsNoTracking();

        // Always split: a single JOIN across Segments + several 1:1 KPI blocks can duplicate rows in the result set.
        // EF may then materialize inconsistent graphs (e.g. KPI range rows tracked as Modified instead of Added),
        // which leads to UPDATE … WHERE Id = … affecting 0 rows (surfaced as DbUpdateConcurrencyException on save).
        query = query.AsSplitQuery();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task SyncSegmentsAsync(
        SegmentationConfiguration entity,
        IReadOnlyList<SegmentationSegmentDto> incoming,
        CancellationToken cancellationToken)
    {
        // Treat Guid.Empty like "no id" so clients never trigger UPDATE by empty PK (DbUpdateConcurrencyException).
        var incomingIds = incoming
            .Select(s => NormalizeSegmentId(s.Id))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        if (entity.Segments.Count > 0 && incomingIds.Count == 0)
        {
            throw new ArgumentException(
                "When updating a configuration that already has segments, each segment in the payload must include its id. " +
                "Omit the id only for brand-new segments you want to add.",
                nameof(incoming));
        }

        foreach (var existing in entity.Segments.ToList())
        {
            if (incomingIds.Contains(existing.Id))
                continue;

            var hasSimulation = await db.SegmentationSimulationFarmers
                .AnyAsync(f => f.SegmentationConfigurationSegmentId == existing.Id, cancellationToken);
            var hasOfficial = await db.FarmerSegmentations
                .AnyAsync(f => f.SegmentationConfigurationSegmentId == existing.Id, cancellationToken);

            if (hasSimulation || hasOfficial)
            {
                throw new InvalidOperationException(
                    $"Segment '{existing.Id}' is referenced by a simulation or official segmentation and cannot be removed.");
            }

            db.SegmentationSegments.Remove(existing);
        }

        // Do not use db.Entry(segment): for an untracked instance it would attach as Unchanged and SaveChanges
        // would emit UPDATE … WHERE Id = @p → 0 rows if that PK is not in the database (DbUpdateConcurrencyException).
        var byId = entity.Segments
            .Where(s => !IsSegmentMarkedDeleted(s))
            .ToDictionary(s => s.Id);
        foreach (var dto in incoming)
        {
            var dtoId = NormalizeSegmentId(dto.Id);
            if (dtoId is { } sid && byId.TryGetValue(sid, out var tracked))
            {
                tracked.SegmentName = dto.SegmentName;
                tracked.RangeMin = dto.RangeMin;
                tracked.OnlyExclusiveFarmer = dto.OnlyExclusiveFarmer;
                tracked.BankDepositDiscount = dto.BankDepositDiscount;
                tracked.TobaccoDiscount = dto.TobaccoDiscount;
                continue;
            }

            var newId = dtoId ?? Guid.NewGuid();
            entity.Segments.Add(new SegmentationSegment
            {
                Id = newId,
                SegmentationConfigurationId = entity.Id,
                SegmentName = dto.SegmentName,
                RangeMin = dto.RangeMin,
                OnlyExclusiveFarmer = dto.OnlyExclusiveFarmer,
                BankDepositDiscount = dto.BankDepositDiscount,
                TobaccoDiscount = dto.TobaccoDiscount
            });
        }
    }

    private static Guid? NormalizeSegmentId(Guid? id) =>
        id is { } g && g != Guid.Empty ? g : null;

    private bool IsSegmentMarkedDeleted(SegmentationSegment segment) =>
        db.ChangeTracker.Entries<SegmentationSegment>()
            .Any(e => ReferenceEquals(e.Entity, segment) && e.State == EntityState.Deleted);

    private void ReplaceLoyaltyRanges(SegmentationConfigurationLoyalty loyalty, SegmentationLoyaltyWriteDto dto)
    {
        // Range rows were already removed from SQLite (see UpdateAsync); collections are empty — only insert new rows.
        foreach (var range in dto.SeasonQuantityRanges)
        {
            var id = Guid.NewGuid();
            var entity = new LoyaltySeasonQuantityRange
            {
                Id = id,
                SegmentationConfigurationId = loyalty.SegmentationConfigurationId,
                PlantingCropSeasonAmount = range.PlantingCropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                MinimumDeliveryAmount = range.MinimumDeliveryAmount,
                MaximumDeliveryAmount = range.MaximumDeliveryAmount,
                DeliveryCropSeasonAmount = range.DeliveryCropSeasonAmount,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new LoyaltySeasonQuantityRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    LoyaltySeasonQuantityRangeId = id,
                    CropSeasonId = cs
                }).ToList()
            };
            loyalty.SeasonQuantityRanges.Add(entity);
        }

        foreach (var h in dto.HistoricalVolumeRanges)
        {
            loyalty.HistoricalVolumeRanges.Add(new LoyaltyHistoricalVolumeRange
            {
                Id = Guid.NewGuid(),
                SegmentationConfigurationId = loyalty.SegmentationConfigurationId,
                MinimumDeliveryAmount = h.MinimumDeliveryAmount,
                MaximumDeliveryAmount = h.MaximumDeliveryAmount,
                Score = h.Score
            });
        }
    }

    private void ReplaceQualityRanges(SegmentationConfigurationQuality quality, SegmentationQualityWriteDto dto)
    {
        foreach (var range in dto.IqsRanges)
        {
            var id = Guid.NewGuid();
            quality.IqsRanges.Add(new QualityIqsRange
            {
                Id = id,
                SegmentationConfigurationId = quality.SegmentationConfigurationId,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new QualityIqsRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    QualityIqsRangeId = id,
                    CropSeasonId = cs
                }).ToList()
            });
        }
    }

    private void ReplaceFinancialRanges(SegmentationConfigurationFinancial financial, SegmentationFinancialWriteDto dto)
    {
        foreach (var range in dto.SelfFundingRanges)
        {
            var id = Guid.NewGuid();
            financial.SelfFundingRanges.Add(new FinancialSelfFundingRange
            {
                Id = id,
                SegmentationConfigurationId = financial.SegmentationConfigurationId,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new FinancialSelfFundingRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    FinancialSelfFundingRangeId = id,
                    CropSeasonId = cs
                }).ToList()
            });
        }
    }

    private void ReplaceYieldRanges(SegmentationConfigurationYield yield, SegmentationYieldWriteDto dto)
    {
        foreach (var range in dto.Ranges)
        {
            var id = Guid.NewGuid();
            yield.Ranges.Add(new YieldRange
            {
                Id = id,
                SegmentationConfigurationId = yield.SegmentationConfigurationId,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new YieldRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    YieldRangeId = id,
                    CropSeasonId = cs
                }).ToList()
            });
        }
    }

    private void ReplaceScaleRanges(SegmentationConfigurationScale scale, SegmentationScaleWriteDto dto)
    {
        foreach (var range in dto.Ranges)
        {
            var id = Guid.NewGuid();
            scale.Ranges.Add(new ScaleRange
            {
                Id = id,
                SegmentationConfigurationId = scale.SegmentationConfigurationId,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new ScaleRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    ScaleRangeId = id,
                    CropSeasonId = cs
                }).ToList()
            });
        }
    }

    private static void ApplyLoyaltyScalars(SegmentationConfigurationLoyalty loyalty, SegmentationLoyaltyWriteDto dto) =>
        loyalty.Relevance = dto.Relevance;

    private static void ApplyQualityScalars(SegmentationConfigurationQuality quality, SegmentationQualityWriteDto dto)
    {
        quality.Relevance = dto.Relevance;
        quality.NtrmCropSeasonAmount = dto.NtrmCropSeasonAmount;
        quality.NtrmCropSeasonStart = dto.NtrmCropSeasonStart;
        quality.NtrmScore = dto.NtrmScore;
        quality.MixtureCropSeasonAmount = dto.MixtureCropSeasonAmount;
        quality.MixtureCropSeasonStart = dto.MixtureCropSeasonStart;
        quality.MixtureScore = dto.MixtureScore;
    }

    private static void ApplyFinancialScalars(SegmentationConfigurationFinancial financial, SegmentationFinancialWriteDto dto)
    {
        financial.Relevance = dto.Relevance;
        financial.DebtCropSeason = dto.DebtCropSeason;
        financial.DebtScore = dto.DebtScore;
    }

    private static void ApplyTechnologyScalars(SegmentationConfigurationTechnology tech, SegmentationTechnologyWriteDto dto)
    {
        tech.Relevance = dto.Relevance;
        tech.HasLargeBaseRidgeWithMulchCropSeason = dto.HasLargeBaseRidgeWithMulchCropSeason;
        tech.HasLargeBaseRidgeWithMulchScore = dto.HasLargeBaseRidgeWithMulchScore;
        tech.HasBroadGrateFurnaceCropSeason = dto.HasBroadGrateFurnaceCropSeason;
        tech.HasBroadGrateFurnaceScore = dto.HasBroadGrateFurnaceScore;
        tech.HasTechnologyPackageAdherenceCropSeason = dto.HasTechnologyPackageAdherenceCropSeason;
        tech.HasTechnologyPackageAdherenceScore = dto.HasTechnologyPackageAdherenceScore;
    }

    private static void ApplyEsgScalars(SegmentationConfigurationEsg esg, SegmentationEsgWriteDto dto)
    {
        esg.Relevance = dto.Relevance;
        esg.ReforestationCropSeason = dto.ReforestationCropSeason;
        esg.ReforestationScorePerPercentualPoint = dto.ReforestationScorePerPercentualPoint;
        esg.ReforestationMaximumScore = dto.ReforestationMaximumScore;
        esg.NativeForestCropSeason = dto.NativeForestCropSeason;
        esg.NativeForestScorePerPercentualPoint = dto.NativeForestScorePerPercentualPoint;
        esg.NativeForestMaximumScore = dto.NativeForestMaximumScore;
        esg.MinorIrregularityCropSeason = dto.MinorIrregularityCropSeason;
        esg.MinorIrregularityScore = dto.MinorIrregularityScore;
        esg.MajorIrregularityCropSeason = dto.MajorIrregularityCropSeason;
        esg.MajorIrregularityScore = dto.MajorIrregularityScore;
    }

    private static void ApplyYieldScalars(SegmentationConfigurationYield yield, SegmentationYieldWriteDto dto) =>
        yield.Relevance = dto.Relevance;

    private static void ApplyScaleScalars(SegmentationConfigurationScale scale, SegmentationScaleWriteDto dto) =>
        scale.Relevance = dto.Relevance;

    private static SegmentationConfiguration MapNewConfiguration(Guid id, SaveSegmentationConfigurationDto dto)
    {
        var loyalty = new SegmentationConfigurationLoyalty
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Loyalty.Relevance,
            MaxScore = 0,
            SeasonQuantityRanges = [],
            HistoricalVolumeRanges = []
        };

        foreach (var range in dto.Loyalty.SeasonQuantityRanges)
        {
            var rangeId = Guid.NewGuid();
            loyalty.SeasonQuantityRanges.Add(new LoyaltySeasonQuantityRange
            {
                Id = rangeId,
                SegmentationConfigurationId = id,
                PlantingCropSeasonAmount = range.PlantingCropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                MinimumDeliveryAmount = range.MinimumDeliveryAmount,
                MaximumDeliveryAmount = range.MaximumDeliveryAmount,
                DeliveryCropSeasonAmount = range.DeliveryCropSeasonAmount,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new LoyaltySeasonQuantityRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    LoyaltySeasonQuantityRangeId = rangeId,
                    CropSeasonId = cs
                }).ToList()
            });
        }

        foreach (var h in dto.Loyalty.HistoricalVolumeRanges)
        {
            loyalty.HistoricalVolumeRanges.Add(new LoyaltyHistoricalVolumeRange
            {
                Id = Guid.NewGuid(),
                SegmentationConfigurationId = id,
                MinimumDeliveryAmount = h.MinimumDeliveryAmount,
                MaximumDeliveryAmount = h.MaximumDeliveryAmount,
                Score = h.Score
            });
        }

        var quality = new SegmentationConfigurationQuality
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Quality.Relevance,
            MaxScore = 0,
            NtrmCropSeasonAmount = dto.Quality.NtrmCropSeasonAmount,
            NtrmCropSeasonStart = dto.Quality.NtrmCropSeasonStart,
            NtrmScore = dto.Quality.NtrmScore,
            MixtureCropSeasonAmount = dto.Quality.MixtureCropSeasonAmount,
            MixtureCropSeasonStart = dto.Quality.MixtureCropSeasonStart,
            MixtureScore = dto.Quality.MixtureScore,
            IqsRanges = []
        };

        foreach (var range in dto.Quality.IqsRanges)
        {
            var rangeId = Guid.NewGuid();
            quality.IqsRanges.Add(new QualityIqsRange
            {
                Id = rangeId,
                SegmentationConfigurationId = id,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new QualityIqsRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    QualityIqsRangeId = rangeId,
                    CropSeasonId = cs
                }).ToList()
            });
        }

        var financial = new SegmentationConfigurationFinancial
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Financial.Relevance,
            MaxScore = 0,
            DebtCropSeason = dto.Financial.DebtCropSeason,
            DebtScore = dto.Financial.DebtScore,
            SelfFundingRanges = []
        };

        foreach (var range in dto.Financial.SelfFundingRanges)
        {
            var rangeId = Guid.NewGuid();
            financial.SelfFundingRanges.Add(new FinancialSelfFundingRange
            {
                Id = rangeId,
                SegmentationConfigurationId = id,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new FinancialSelfFundingRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    FinancialSelfFundingRangeId = rangeId,
                    CropSeasonId = cs
                }).ToList()
            });
        }

        var technology = new SegmentationConfigurationTechnology
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Technology.Relevance,
            MaxScore = 0,
            HasLargeBaseRidgeWithMulchCropSeason = dto.Technology.HasLargeBaseRidgeWithMulchCropSeason,
            HasLargeBaseRidgeWithMulchScore = dto.Technology.HasLargeBaseRidgeWithMulchScore,
            HasBroadGrateFurnaceCropSeason = dto.Technology.HasBroadGrateFurnaceCropSeason,
            HasBroadGrateFurnaceScore = dto.Technology.HasBroadGrateFurnaceScore,
            HasTechnologyPackageAdherenceCropSeason = dto.Technology.HasTechnologyPackageAdherenceCropSeason,
            HasTechnologyPackageAdherenceScore = dto.Technology.HasTechnologyPackageAdherenceScore
        };

        var esg = new SegmentationConfigurationEsg
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Esg.Relevance,
            MaxScore = 0,
            ReforestationCropSeason = dto.Esg.ReforestationCropSeason,
            ReforestationScorePerPercentualPoint = dto.Esg.ReforestationScorePerPercentualPoint,
            ReforestationMaximumScore = dto.Esg.ReforestationMaximumScore,
            NativeForestCropSeason = dto.Esg.NativeForestCropSeason,
            NativeForestScorePerPercentualPoint = dto.Esg.NativeForestScorePerPercentualPoint,
            NativeForestMaximumScore = dto.Esg.NativeForestMaximumScore,
            MinorIrregularityCropSeason = dto.Esg.MinorIrregularityCropSeason,
            MinorIrregularityScore = dto.Esg.MinorIrregularityScore,
            MajorIrregularityCropSeason = dto.Esg.MajorIrregularityCropSeason,
            MajorIrregularityScore = dto.Esg.MajorIrregularityScore
        };

        var yield = new SegmentationConfigurationYield
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Yield.Relevance,
            MaxScore = 0,
            Ranges = []
        };

        foreach (var range in dto.Yield.Ranges)
        {
            var rangeId = Guid.NewGuid();
            yield.Ranges.Add(new YieldRange
            {
                Id = rangeId,
                SegmentationConfigurationId = id,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new YieldRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    YieldRangeId = rangeId,
                    CropSeasonId = cs
                }).ToList()
            });
        }

        var scale = new SegmentationConfigurationScale
        {
            SegmentationConfigurationId = id,
            Relevance = dto.Scale.Relevance,
            MaxScore = 0,
            Ranges = []
        };

        foreach (var range in dto.Scale.Ranges)
        {
            var rangeId = Guid.NewGuid();
            scale.Ranges.Add(new ScaleRange
            {
                Id = rangeId,
                SegmentationConfigurationId = id,
                Minimum = range.Minimum,
                Maximum = range.Maximum,
                CropSeasonAmount = range.CropSeasonAmount,
                CropSeasonStart = range.CropSeasonStart,
                Score = range.Score,
                SkippedCropSeasons = range.SkippedCropSeasonIds.Select(cs => new ScaleRangeSkippedCropSeason
                {
                    Id = Guid.NewGuid(),
                    ScaleRangeId = rangeId,
                    CropSeasonId = cs
                }).ToList()
            });
        }

        return new SegmentationConfiguration
        {
            Id = id,
            Name = dto.Name,
            MaximumScore = dto.MaximumScore,
            Segments = dto.Segments.Select(s => new SegmentationSegment
            {
                Id = s.Id ?? Guid.NewGuid(),
                SegmentationConfigurationId = id,
                SegmentName = s.SegmentName,
                RangeMin = s.RangeMin,
                OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                BankDepositDiscount = s.BankDepositDiscount,
                TobaccoDiscount = s.TobaccoDiscount
            }).ToList(),
            Loyalty = loyalty,
            Quality = quality,
            Financial = financial,
            Technology = technology,
            Esg = esg,
            Yield = yield,
            Scale = scale
        };
    }

    private static SegmentationConfiguration CloneConfiguration(SegmentationConfiguration source, Guid newId, string name)
    {
        var dto = MapToSaveDto(source, name);
        return MapNewConfiguration(newId, dto);
    }

    private static SaveSegmentationConfigurationDto MapToSaveDto(SegmentationConfiguration c, string? nameOverride = null)
    {
        return new SaveSegmentationConfigurationDto
        {
            Name = nameOverride ?? c.Name,
            MaximumScore = c.MaximumScore,
            Segments = c.Segments
                .OrderBy(s => s.RangeMin ?? int.MaxValue)
                .Select(s => new SegmentationSegmentDto
                {
                    Id = null,
                    SegmentName = s.SegmentName,
                    RangeMin = s.RangeMin,
                    OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                    BankDepositDiscount = s.BankDepositDiscount,
                    TobaccoDiscount = s.TobaccoDiscount
                }).ToList(),
            Loyalty = MapLoyaltyWrite(c.Loyalty!),
            Quality = MapQualityWrite(c.Quality!),
            Financial = MapFinancialWrite(c.Financial!),
            Technology = MapTechnologyWrite(c.Technology!),
            Esg = MapEsgWrite(c.Esg!),
            Yield = MapYieldWrite(c.Yield!),
            Scale = MapScaleWrite(c.Scale!)
        };
    }

    private static SegmentationLoyaltyWriteDto MapLoyaltyWrite(SegmentationConfigurationLoyalty l) => new()
    {
        Relevance = l.Relevance,
        SeasonQuantityRanges = l.SeasonQuantityRanges
            .Select(r => new LoyaltySeasonQuantityRangeDto
            {
                PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
                CropSeasonStart = r.CropSeasonStart,
                MinimumDeliveryAmount = r.MinimumDeliveryAmount,
                MaximumDeliveryAmount = r.MaximumDeliveryAmount,
                DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
                Score = r.Score,
                SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
            }).ToList(),
        HistoricalVolumeRanges = l.HistoricalVolumeRanges
            .Select(h => new LoyaltyHistoricalVolumeRangeDto
            {
                MinimumDeliveryAmount = h.MinimumDeliveryAmount,
                MaximumDeliveryAmount = h.MaximumDeliveryAmount,
                Score = h.Score
            }).ToList()
    };

    private static SegmentationQualityWriteDto MapQualityWrite(SegmentationConfigurationQuality q) => new()
    {
        Relevance = q.Relevance,
        NtrmCropSeasonAmount = q.NtrmCropSeasonAmount,
        NtrmCropSeasonStart = q.NtrmCropSeasonStart,
        NtrmScore = q.NtrmScore,
        MixtureCropSeasonAmount = q.MixtureCropSeasonAmount,
        MixtureCropSeasonStart = q.MixtureCropSeasonStart,
        MixtureScore = q.MixtureScore,
        IqsRanges = q.IqsRanges
            .Select(r => new QualityIqsRangeDto
            {
                Minimum = r.Minimum,
                Maximum = r.Maximum,
                CropSeasonAmount = r.CropSeasonAmount,
                CropSeasonStart = r.CropSeasonStart,
                Score = r.Score,
                SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
            }).ToList()
    };

    private static SegmentationFinancialWriteDto MapFinancialWrite(SegmentationConfigurationFinancial f) => new()
    {
        Relevance = f.Relevance,
        DebtCropSeason = f.DebtCropSeason,
        DebtScore = f.DebtScore,
        SelfFundingRanges = f.SelfFundingRanges
            .Select(r => new FinancialSelfFundingRangeDto
            {
                Minimum = r.Minimum,
                Maximum = r.Maximum,
                CropSeasonAmount = r.CropSeasonAmount,
                CropSeasonStart = r.CropSeasonStart,
                Score = r.Score,
                SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
            }).ToList()
    };

    private static SegmentationTechnologyWriteDto MapTechnologyWrite(SegmentationConfigurationTechnology t) => new()
    {
        Relevance = t.Relevance,
        HasLargeBaseRidgeWithMulchCropSeason = t.HasLargeBaseRidgeWithMulchCropSeason,
        HasLargeBaseRidgeWithMulchScore = t.HasLargeBaseRidgeWithMulchScore,
        HasBroadGrateFurnaceCropSeason = t.HasBroadGrateFurnaceCropSeason,
        HasBroadGrateFurnaceScore = t.HasBroadGrateFurnaceScore,
        HasTechnologyPackageAdherenceCropSeason = t.HasTechnologyPackageAdherenceCropSeason,
        HasTechnologyPackageAdherenceScore = t.HasTechnologyPackageAdherenceScore
    };

    private static SegmentationEsgWriteDto MapEsgWrite(SegmentationConfigurationEsg e) => new()
    {
        Relevance = e.Relevance,
        ReforestationCropSeason = e.ReforestationCropSeason,
        ReforestationScorePerPercentualPoint = e.ReforestationScorePerPercentualPoint,
        ReforestationMaximumScore = e.ReforestationMaximumScore,
        NativeForestCropSeason = e.NativeForestCropSeason,
        NativeForestScorePerPercentualPoint = e.NativeForestScorePerPercentualPoint,
        NativeForestMaximumScore = e.NativeForestMaximumScore,
        MinorIrregularityCropSeason = e.MinorIrregularityCropSeason,
        MinorIrregularityScore = e.MinorIrregularityScore,
        MajorIrregularityCropSeason = e.MajorIrregularityCropSeason,
        MajorIrregularityScore = e.MajorIrregularityScore
    };

    private static SegmentationYieldWriteDto MapYieldWrite(SegmentationConfigurationYield y) => new()
    {
        Relevance = y.Relevance,
        Ranges = y.Ranges
            .Select(r => new YieldRangeDto
            {
                Minimum = r.Minimum,
                Maximum = r.Maximum,
                CropSeasonAmount = r.CropSeasonAmount,
                CropSeasonStart = r.CropSeasonStart,
                Score = r.Score,
                SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
            }).ToList()
    };

    private static SegmentationScaleWriteDto MapScaleWrite(SegmentationConfigurationScale s) => new()
    {
        Relevance = s.Relevance,
        Ranges = s.Ranges
            .Select(r => new ScaleRangeDto
            {
                Minimum = r.Minimum,
                Maximum = r.Maximum,
                CropSeasonAmount = r.CropSeasonAmount,
                CropSeasonStart = r.CropSeasonStart,
                Score = r.Score,
                SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
            }).ToList()
    };

    private static SegmentationConfigurationDetailDto MapToDetail(SegmentationConfiguration c)
    {
        SegmentationConfigurationKpiMaxScores.SynchronizeFromRules(c);

        return new SegmentationConfigurationDetailDto
        {
            Id = c.Id,
            Name = c.Name,
            MaximumScore = c.MaximumScore,
            Segments = c.Segments
                .OrderBy(s => s.RangeMin ?? int.MaxValue)
                .Select(s => new SegmentationSegmentDto
                {
                    Id = s.Id,
                    SegmentName = s.SegmentName,
                    RangeMin = s.RangeMin,
                    OnlyExclusiveFarmer = s.OnlyExclusiveFarmer,
                    BankDepositDiscount = s.BankDepositDiscount,
                    TobaccoDiscount = s.TobaccoDiscount
                }).ToList(),
            Loyalty = new SegmentationLoyaltyDetailDto
            {
                MaxScore = c.Loyalty!.MaxScore,
                Relevance = c.Loyalty.Relevance,
                SeasonQuantityRanges = c.Loyalty.SeasonQuantityRanges
                    .Select(r => new LoyaltySeasonQuantityRangeDto
                    {
                        PlantingCropSeasonAmount = r.PlantingCropSeasonAmount,
                        CropSeasonStart = r.CropSeasonStart,
                        MinimumDeliveryAmount = r.MinimumDeliveryAmount,
                        MaximumDeliveryAmount = r.MaximumDeliveryAmount,
                        DeliveryCropSeasonAmount = r.DeliveryCropSeasonAmount,
                        Score = r.Score,
                        SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
                    }).ToList(),
                HistoricalVolumeRanges = c.Loyalty.HistoricalVolumeRanges
                    .Select(h => new LoyaltyHistoricalVolumeRangeDto
                    {
                        MinimumDeliveryAmount = h.MinimumDeliveryAmount,
                        MaximumDeliveryAmount = h.MaximumDeliveryAmount,
                        Score = h.Score
                    }).ToList()
            },
            Quality = new SegmentationQualityDetailDto
            {
                MaxScore = c.Quality!.MaxScore,
                Relevance = c.Quality.Relevance,
                NtrmCropSeasonAmount = c.Quality.NtrmCropSeasonAmount,
                NtrmCropSeasonStart = c.Quality.NtrmCropSeasonStart,
                NtrmScore = c.Quality.NtrmScore,
                MixtureCropSeasonAmount = c.Quality.MixtureCropSeasonAmount,
                MixtureCropSeasonStart = c.Quality.MixtureCropSeasonStart,
                MixtureScore = c.Quality.MixtureScore,
                IqsRanges = c.Quality.IqsRanges
                    .Select(r => new QualityIqsRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        CropSeasonStart = r.CropSeasonStart,
                        Score = r.Score,
                        SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
                    }).ToList()
            },
            Financial = new SegmentationFinancialDetailDto
            {
                MaxScore = c.Financial!.MaxScore,
                Relevance = c.Financial.Relevance,
                DebtCropSeason = c.Financial.DebtCropSeason,
                DebtScore = c.Financial.DebtScore,
                SelfFundingRanges = c.Financial.SelfFundingRanges
                    .Select(r => new FinancialSelfFundingRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        CropSeasonStart = r.CropSeasonStart,
                        Score = r.Score,
                        SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
                    }).ToList()
            },
            Technology = new SegmentationTechnologyDetailDto
            {
                MaxScore = c.Technology!.MaxScore,
                Relevance = c.Technology.Relevance,
                HasLargeBaseRidgeWithMulchCropSeason = c.Technology.HasLargeBaseRidgeWithMulchCropSeason,
                HasLargeBaseRidgeWithMulchScore = c.Technology.HasLargeBaseRidgeWithMulchScore,
                HasBroadGrateFurnaceCropSeason = c.Technology.HasBroadGrateFurnaceCropSeason,
                HasBroadGrateFurnaceScore = c.Technology.HasBroadGrateFurnaceScore,
                HasTechnologyPackageAdherenceCropSeason = c.Technology.HasTechnologyPackageAdherenceCropSeason,
                HasTechnologyPackageAdherenceScore = c.Technology.HasTechnologyPackageAdherenceScore
            },
            Esg = new SegmentationEsgDetailDto
            {
                MaxScore = c.Esg!.MaxScore,
                Relevance = c.Esg.Relevance,
                ReforestationCropSeason = c.Esg.ReforestationCropSeason,
                ReforestationScorePerPercentualPoint = c.Esg.ReforestationScorePerPercentualPoint,
                ReforestationMaximumScore = c.Esg.ReforestationMaximumScore,
                NativeForestCropSeason = c.Esg.NativeForestCropSeason,
                NativeForestScorePerPercentualPoint = c.Esg.NativeForestScorePerPercentualPoint,
                NativeForestMaximumScore = c.Esg.NativeForestMaximumScore,
                MinorIrregularityCropSeason = c.Esg.MinorIrregularityCropSeason,
                MinorIrregularityScore = c.Esg.MinorIrregularityScore,
                MajorIrregularityCropSeason = c.Esg.MajorIrregularityCropSeason,
                MajorIrregularityScore = c.Esg.MajorIrregularityScore
            },
            Yield = new SegmentationYieldDetailDto
            {
                MaxScore = c.Yield!.MaxScore,
                Relevance = c.Yield.Relevance,
                Ranges = c.Yield.Ranges
                    .Select(r => new YieldRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        CropSeasonStart = r.CropSeasonStart,
                        Score = r.Score,
                        SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
                    }).ToList()
            },
            Scale = new SegmentationScaleDetailDto
            {
                MaxScore = c.Scale!.MaxScore,
                Relevance = c.Scale.Relevance,
                Ranges = c.Scale.Ranges
                    .Select(r => new ScaleRangeDto
                    {
                        Minimum = r.Minimum,
                        Maximum = r.Maximum,
                        CropSeasonAmount = r.CropSeasonAmount,
                        CropSeasonStart = r.CropSeasonStart,
                        Score = r.Score,
                        SkippedCropSeasonIds = r.SkippedCropSeasons.Select(x => x.CropSeasonId).ToList()
                    }).ToList()
            }
        };
    }

    /// <summary>
    /// After <see cref="DeleteAllKpiRangeRowsForConfigurationSqlAsync"/>, every KPI range row in this unit of work
    /// must be inserted. If the change tracker marks any of those rows as <see cref="EntityState.Modified"/>, EF
    /// will emit <c>UPDATE</c> instead of <c>INSERT</c> and SQLite returns 0 rows (DbUpdateConcurrencyException).
    /// </summary>
    private void NormalizeKpiRangeRowStatesAfterSqlDeletes()
    {
        foreach (EntityEntry entry in db.ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Modified)
                continue;

            if (entry.Entity is LoyaltySeasonQuantityRange
                or LoyaltyHistoricalVolumeRange
                or LoyaltySeasonQuantityRangeSkippedCropSeason
                or QualityIqsRange
                or QualityIqsRangeSkippedCropSeason
                or FinancialSelfFundingRange
                or FinancialSelfFundingRangeSkippedCropSeason
                or YieldRange
                or YieldRangeSkippedCropSeason
                or ScaleRange
                or ScaleRangeSkippedCropSeason)
            {
                entry.State = EntityState.Added;
            }
        }
    }

    /// <summary>
    /// Keeps stored <see cref="SegmentationConfigurationLoyalty.Relevance"/> (etc.) equal to
    /// <c>MaxScore / MaximumScore</c> after rules are synchronized.
    /// </summary>
    private void AssignRelevanceFromMaxScores(SegmentationConfiguration c)
    {
        var m = (decimal)c.MaximumScore;
        if (m <= 0)
            return;
        c.Loyalty!.Relevance = (decimal)c.Loyalty.MaxScore / m;
        c.Quality!.Relevance = (decimal)c.Quality.MaxScore / m;
        c.Financial!.Relevance = (decimal)c.Financial.MaxScore / m;
        c.Technology!.Relevance = (decimal)c.Technology.MaxScore / m;
        c.Esg!.Relevance = (decimal)c.Esg.MaxScore / m;
        c.Yield!.Relevance = (decimal)c.Yield.MaxScore / m;
        c.Scale!.Relevance = (decimal)c.Scale.MaxScore / m;
    }
}
