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

    public Task<KpiImportResultDto> ImportYieldAndScaleAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseYieldAndScaleRow, UpsertYieldAndScaleAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportTechnologiesAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseTechnologiesRow, UpsertTechnologiesAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportEsgAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseEsgRow, UpsertEsgAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportEsgIrregularitiesAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseEsgIrregularityRow, UpsertEsgIrregularityAsync, cancellationToken);

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
        var deliveredPct = GetRequiredInt(csv, "deliveredpercentage");
        var deliveredKg = GetRequiredInt(csv, "deliveredamountkg");
        var contractedKg = GetRequiredInt(csv, "contractedamountkg");
        return new LoyaltyParsed(farmerCode, cropSeasonId, cultureType, deliveredPct, deliveredKg, contractedKg);
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

    private static YieldAndScaleParsed ParseYieldAndScaleRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var yield = GetRequiredInt(csv, "yield");
        var scale = GetRequiredInt(csv, "scale");
        var contracted = GetRequiredInt(csv, "contractedamountkg");
        return new YieldAndScaleParsed(farmerCode, cropSeasonId, cultureType, yield, scale, contracted);
    }

    private static TechnologiesParsed ParseTechnologiesRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var technologyId = GetRequiredInt(csv, "technologyid");
        return new TechnologiesParsed(farmerCode, cropSeasonId, cultureType, technologyId);
    }

    private static EsgParsed ParseEsgRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var reforestation = GetRequiredInt(csv, "reforestationpercentage");
        var nativeForest = GetRequiredInt(csv, "nativeforestpercentage");
        return new EsgParsed(farmerCode, cropSeasonId, cultureType, reforestation, nativeForest);
    }

    private static EsgIrregularityParsed ParseEsgIrregularityRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var irregularityTypeId = GetRequiredInt(csv, "irregularitytypeid");
        return new EsgIrregularityParsed(farmerCode, cropSeasonId, cultureType, irregularityTypeId);
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
                DeliveredPercentage = row.DeliveredPercentage,
                DeliveredAmountKg = row.DeliveredAmountKg,
                ContractedAmountKg = row.ContractedAmountKg
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.DeliveredPercentage = row.DeliveredPercentage;
        existing.DeliveredAmountKg = row.DeliveredAmountKg;
        existing.ContractedAmountKg = row.ContractedAmountKg;
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

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertYieldAndScaleAsync(
        YieldAndScaleParsed row,
        CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var existing = await db.YieldAndScaleKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.YieldAndScaleKpis.Add(new YieldAndScaleKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                Yield = row.Yield,
                Scale = row.Scale,
                ContractedAmountKg = row.ContractedAmountKg
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.Yield = row.Yield;
        existing.Scale = row.Scale;
        existing.ContractedAmountKg = row.ContractedAmountKg;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertTechnologiesAsync(
        TechnologiesParsed row,
        CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        if (!await db.Technologies.AnyAsync(t => t.Id == row.TechnologyId, cancellationToken))
            return (false, false, new KpiImportErrorDto(0, $"Unknown technologyId '{row.TechnologyId}'.", row.FarmerCode, row.CropSeasonId));

        var existing = await db.TechnologiesKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId
                 && k.CropSeasonId == row.CropSeasonId
                 && k.CultureTypeCode == row.CultureTypeCode
                 && k.TechnologyId == row.TechnologyId,
            cancellationToken);

        if (existing is null)
        {
            db.TechnologiesKpis.Add(new TechnologiesKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                TechnologyId = row.TechnologyId
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        return (false, true, null);
    }

    private static bool? GetOptionalBool(CsvReader csv, string normalizedName)
    {
        try
        {
            var raw = csv.GetField(normalizedName);
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            if (bool.TryParse(raw, out var b))
                return b;
            if (raw == "1") return true;
            if (raw == "0") return false;
            return null;
        }
        catch
        {
            return null;
        }
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
                NativeForestPercentage = row.NativeForestPercentage
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.ReforestationPercentage = row.ReforestationPercentage;
        existing.NativeForestPercentage = row.NativeForestPercentage;
        await db.SaveChangesAsync(cancellationToken);
        return (false, true, null);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertEsgIrregularityAsync(
        EsgIrregularityParsed row,
        CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        if (!await db.IrregularityTypes.AnyAsync(t => t.Id == row.IrregularityTypeId, cancellationToken))
            return (false, false, new KpiImportErrorDto(0, $"Unknown irregularityTypeId '{row.IrregularityTypeId}'.", row.FarmerCode, row.CropSeasonId));

        var existing = await db.EsgIrregularityKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId
                 && k.CropSeasonId == row.CropSeasonId
                 && k.CultureTypeCode == row.CultureTypeCode
                 && k.IrregularityTypeId == row.IrregularityTypeId,
            cancellationToken);

        if (existing is null)
        {
            db.EsgIrregularityKpis.Add(new EsgIrregularityKpi
            {
                Id = Guid.NewGuid(),
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                IrregularityTypeId = row.IrregularityTypeId
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

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

    private sealed record LoyaltyParsed(
        string FarmerCode,
        int CropSeasonId,
        string CultureTypeCode,
        int DeliveredPercentage,
        int DeliveredAmountKg,
        int ContractedAmountKg);

    private sealed record QualityParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int Iqs, bool HadNtrm, bool HadQualityMixture);
    private sealed record FinancialParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int SelfFundingPercentage, bool HaveDebt);
    private sealed record YieldAndScaleParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int Yield, int Scale, int ContractedAmountKg);
    private sealed record TechnologiesParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int TechnologyId);
    private sealed record EsgParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int ReforestationPercentage, int NativeForestPercentage);
    private sealed record EsgIrregularityParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int IrregularityTypeId);
}
