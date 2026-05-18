using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Domain.Entities;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Infrastructure.Services;

public sealed class KpiImportService(AppDbContext db) : IKpiImportService
{
    public Task<KpiImportResultDto> ImportLoyaltyAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseLoyaltyRow, UpsertLoyaltyAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportQualityAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseQualityRow, UpsertQualityAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportFinancialAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseFinancialRow, UpsertFinancialAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportYieldAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseYieldRow, UpsertYieldAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportScaleAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseScaleRow, UpsertScaleAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportTechnologiesAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseTechnologiesRow, UpsertTechnologiesAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportEsgAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseEsgRow, UpsertEsgAsync, cancellationToken);

    private async Task<KpiImportResultDto> ImportAsync<TParsed>(
        Stream csvStream,
        Func<CsvReader, TParsed> parseRow,
        Func<TParsed, CancellationToken, Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)>> upsert,
        CancellationToken cancellationToken)
    {
        var errors = new List<KpiImportErrorDto>();
        var inserted = 0;
        var updated = 0;
        var totalRows = 0;

        using var reader = new StreamReader(csvStream, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            PrepareHeaderForMatch = args =>
                new string(args.Header.Where(ch => ch != '_' && !char.IsWhiteSpace(ch)).ToArray()).ToLowerInvariant()
        };

        using var csv = new CsvReader(reader, config);

        if (!await csv.ReadAsync() || !csv.ReadHeader())
        {
            return new KpiImportResultDto(
                TotalRows: 0,
                InsertedRows: 0,
                UpdatedRows: 0,
                Errors: new[] { new KpiImportErrorDto(0, "CSV file is empty or missing a header row.") });
        }

        while (await csv.ReadAsync())
        {
            totalRows++;
            try
            {
                var parsed = parseRow(csv);

                var (Inserted, Updated, Error) = await upsert(parsed, cancellationToken);
                if (Error is not null)
                {
                    errors.Add(Error with { RowNumber = (int)csv.Parser.RawRow });
                    continue;
                }

                if (Inserted) inserted++;
                if (Updated) updated++;
            }
            catch (Exception ex)
            {
                errors.Add(new KpiImportErrorDto((int)csv.Parser.RawRow, $"Failed to read row: {ex.Message}"));
            }
        }

        return new KpiImportResultDto(totalRows, inserted, updated, errors);
    }

    // -------------------------
    // Parsing
    // -------------------------

    private static LoyaltyParsed ParseLoyaltyRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var delivered = GetRequiredInt(csv, "deliveredpercentage");
        return new LoyaltyParsed(farmerCode, cropSeasonId, cultureType, delivered);
    }

    private static QualityParsed ParseQualityRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var iqs = GetRequiredInt(csv, "iqs");
        var hadNtrm = GetRequiredBool(csv, "hadntrm");
        var hadMixture = GetRequiredBool(csv, "hadqualitymixture");
        return new QualityParsed(farmerCode, cropSeasonId, cultureType, iqs, hadNtrm, hadMixture);
    }

    private static FinancialParsed ParseFinancialRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var selfFunding = GetRequiredInt(csv, "selffundingpercentage");
        var haveDebt = GetRequiredBool(csv, "havedebt");
        return new FinancialParsed(farmerCode, cropSeasonId, cultureType, selfFunding, haveDebt);
    }

    private static YieldParsed ParseYieldRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var yield = GetRequiredInt(csv, "yield");
        return new YieldParsed(farmerCode, cropSeasonId, cultureType, yield);
    }

    private static ScaleParsed ParseScaleRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var scale = GetRequiredInt(csv, "scale");
        return new ScaleParsed(farmerCode, cropSeasonId, cultureType, scale);
    }

    private static TechnologiesParsed ParseTechnologiesRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var mulch = GetRequiredBool(csv, "haslargebaseridgewithmulch");
        var furnace = GetRequiredBool(csv, "hasbroadgratefurnace");
        var pkg = GetRequiredBool(csv, "hastechnologypackageadherence");
        return new TechnologiesParsed(farmerCode, cropSeasonId, cultureType, mulch, furnace, pkg);
    }

    private static EsgParsed ParseEsgRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var reforestation = GetRequiredInt(csv, "reforestationpercentage");
        var nativeForest = GetRequiredInt(csv, "nativeforestpercentage");
        var minor = GetRequiredBool(csv, "hasminorirregularity");
        var major = GetRequiredBool(csv, "hasmajorirregularity");
        return new EsgParsed(farmerCode, cropSeasonId, cultureType, reforestation, nativeForest, minor, major);
    }

    // -------------------------
    // Upsert
    // -------------------------

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertLoyaltyAsync(LoyaltyParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.LoyaltyKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.LoyaltyKpis.Add(new LoyaltyKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                DeliveredPercentage = row.DeliveredPercentage
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.DeliveredPercentage = row.DeliveredPercentage;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertQualityAsync(QualityParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.QualityKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.QualityKpis.Add(new QualityKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                Iqs = row.Iqs,
                HadNtrm = row.HadNtrm,
                HadQualityMixture = row.HadQualityMixture
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.Iqs = row.Iqs;
        existing.HadNtrm = row.HadNtrm;
        existing.HadQualityMixture = row.HadQualityMixture;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertFinancialAsync(FinancialParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.FinancialKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.FinancialKpis.Add(new FinancialKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                SelfFundingPercentage = row.SelfFundingPercentage,
                HaveDebt = row.HaveDebt
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.SelfFundingPercentage = row.SelfFundingPercentage;
        existing.HaveDebt = row.HaveDebt;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertYieldAsync(YieldParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.YieldKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.YieldKpis.Add(new YieldKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                Yield = row.Yield
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.Yield = row.Yield;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertScaleAsync(ScaleParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.ScaleKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.ScaleKpis.Add(new ScaleKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                Scale = row.Scale
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.Scale = row.Scale;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertTechnologiesAsync(TechnologiesParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.TechnologiesKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.TechnologiesKpis.Add(new TechnologiesKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                HasLargeBaseRidgeWithMulch = row.HasLargeBaseRidgeWithMulch,
                HasBroadGrateFurnace = row.HasBroadGrateFurnace,
                HasTechnologyPackageAdherence = row.HasTechnologyPackageAdherence
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.HasLargeBaseRidgeWithMulch = row.HasLargeBaseRidgeWithMulch;
        existing.HasBroadGrateFurnace = row.HasBroadGrateFurnace;
        existing.HasTechnologyPackageAdherence = row.HasTechnologyPackageAdherence;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertEsgAsync(EsgParsed row, CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.EsgKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.EsgKpis.Add(new EsgKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                ReforestationPercentage = row.ReforestationPercentage,
                NativeForestPercentage = row.NativeForestPercentage,
                HasMinorIrregularity = row.HasMinorIrregularity,
                HasMajorIrregularity = row.HasMajorIrregularity
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.ReforestationPercentage = row.ReforestationPercentage;
        existing.NativeForestPercentage = row.NativeForestPercentage;
        existing.HasMinorIrregularity = row.HasMinorIrregularity;
        existing.HasMajorIrregularity = row.HasMajorIrregularity;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(Guid FarmerId, KpiImportErrorDto? Error)> ResolveFarmerAndSeasonAsync(
        string farmerCode,
        int cropSeasonId,
        CancellationToken cancellationToken)
    {
        var existsSeason = await db.CropSeasons.AsNoTracking().AnyAsync(c => c.Id == cropSeasonId, cancellationToken);
        if (!existsSeason)
            return (Guid.Empty, new KpiImportErrorDto(0, $"Unknown cropSeasonId '{cropSeasonId}'.", farmerCode, cropSeasonId));

        var farmerId = await db.Farmers.AsNoTracking()
            .Where(f => f.Code == farmerCode)
            .Select(f => (Guid?)f.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (farmerId is null)
            return (Guid.Empty, new KpiImportErrorDto(0, $"Unknown farmerCode '{farmerCode}'.", farmerCode, cropSeasonId));

        return (farmerId.Value, null);
    }

    // -------------------------
    // CSV field helpers
    // -------------------------

    private static string GetRequiredString(CsvReader csv, string normalizedName)
    {
        var value = csv.GetField(normalizedName);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing required field: {normalizedName}.");
        return value.Trim();
    }

    private static string? GetOptionalString(CsvReader csv, string normalizedName)
    {
        try
        {
            var value = csv.GetField(normalizedName);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static int GetRequiredInt(CsvReader csv, string normalizedName)
    {
        var raw = GetRequiredString(csv, normalizedName);
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            throw new InvalidOperationException($"Invalid integer '{raw}' for field {normalizedName}.");
        return value;
    }

    private static bool GetRequiredBool(CsvReader csv, string normalizedName)
    {
        var raw = GetRequiredString(csv, normalizedName);
        if (bool.TryParse(raw, out var b))
            return b;
        if (raw == "1") return true;
        if (raw == "0") return false;
        throw new InvalidOperationException($"Invalid boolean '{raw}' for field {normalizedName}.");
    }

    // -------------------------
    // Parsed row models
    // -------------------------

    private sealed record LoyaltyParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int DeliveredPercentage);
    private sealed record QualityParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int Iqs, bool HadNtrm, bool HadQualityMixture);
    private sealed record FinancialParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int SelfFundingPercentage, bool HaveDebt);
    private sealed record YieldParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int Yield);
    private sealed record ScaleParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int Scale);
    private sealed record TechnologiesParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, bool HasLargeBaseRidgeWithMulch, bool HasBroadGrateFurnace, bool HasTechnologyPackageAdherence);
    private sealed record EsgParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int ReforestationPercentage, int NativeForestPercentage, bool HasMinorIrregularity, bool HasMajorIrregularity);
}
