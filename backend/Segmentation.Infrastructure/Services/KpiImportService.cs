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
    public Task<KpiImportResultDto> ImportFarmerContractKpisAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseContractRow, UpsertContractAsync, cancellationToken);

    public Task<KpiImportResultDto> ImportTechnologiesAsync(Stream csvStream, CancellationToken cancellationToken = default) =>
        ImportAsync(csvStream, ParseTechnologiesRow, UpsertTechnologiesAsync, cancellationToken);

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

    private static ContractParsed ParseContractRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var deliveredPct = GetRequiredInt(csv, "deliveredpercentage");
        var deliveredKg = GetOptionalInt(csv, "deliveredamountkg")
            ?? (int)Math.Round(deliveredPct * 100m, MidpointRounding.AwayFromZero);
        var contractedKg = GetRequiredInt(csv, "contractedamountkg");
        return new ContractParsed(
            farmerCode,
            cropSeasonId,
            cultureType,
            deliveredPct,
            deliveredKg,
            contractedKg,
            GetRequiredInt(csv, "iqs"),
            GetRequiredBool(csv, "hadntrm"),
            GetRequiredBool(csv, "hadqualitymixture"),
            GetRequiredInt(csv, "selffundingpercentage"),
            GetRequiredBool(csv, "havedebt"),
            GetRequiredInt(csv, "yield"),
            GetRequiredInt(csv, "scale"),
            GetRequiredInt(csv, "reforestationpercentage"),
            GetRequiredInt(csv, "nativeforestpercentage"));
    }

    private static TechnologiesParsed ParseTechnologiesRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var technologyId = GetRequiredInt(csv, "technologyid");
        return new TechnologiesParsed(farmerCode, cropSeasonId, cultureType, technologyId);
    }

    private static EsgIrregularityParsed ParseEsgIrregularityRow(CsvReader csv)
    {
        var farmerCode = GetRequiredString(csv, "farmercode");
        var cropSeasonId = GetRequiredInt(csv, "cropseasonid");
        var cultureType = GetOptionalString(csv, "culturetypecode") ?? "FCV";
        var irregularityTypeId = GetRequiredInt(csv, "irregularitytypeid");
        return new EsgIrregularityParsed(farmerCode, cropSeasonId, cultureType, irregularityTypeId);
    }

    private async Task<(bool Inserted, bool Updated, KpiImportErrorDto? Error)> UpsertContractAsync(
        ContractParsed row,
        CancellationToken cancellationToken)
    {
        var (farmerId, error) = await ResolveFarmerAndSeasonAsync(row.FarmerCode, row.CropSeasonId, cancellationToken);
        if (error is not null) return (false, false, error);

        var farmer = await db.Farmers.FirstAsync(f => f.Id == farmerId, cancellationToken);
        var existing = await db.FarmerContractKpis.FirstOrDefaultAsync(
            k => k.FarmerId == farmerId && k.CropSeasonId == row.CropSeasonId && k.CultureTypeCode == row.CultureTypeCode,
            cancellationToken);

        if (existing is null)
        {
            db.FarmerContractKpis.Add(new FarmerContractKpi
            {
                FarmerId = farmerId,
                CropSeasonId = row.CropSeasonId,
                CultureTypeCode = row.CultureTypeCode,
                DeliveredPercentage = row.DeliveredPercentage,
                DeliveredAmountKg = row.DeliveredAmountKg,
                ContractedAmountKg = row.ContractedAmountKg,
                Iqs = row.Iqs,
                HadNtrm = row.HadNtrm,
                HadQualityMixture = row.HadQualityMixture,
                SelfFundingPercentage = row.SelfFundingPercentage,
                HaveDebt = row.HaveDebt,
                Yield = row.Yield,
                Scale = row.Scale,
                ReforestationPercentage = row.ReforestationPercentage,
                NativeForestPercentage = row.NativeForestPercentage,
                NonExclusive = farmer.NonExclusiveFarmer
            });
            await db.SaveChangesAsync(cancellationToken);
            return (true, false, null);
        }

        existing.DeliveredPercentage = row.DeliveredPercentage;
        existing.DeliveredAmountKg = row.DeliveredAmountKg;
        existing.ContractedAmountKg = row.ContractedAmountKg;
        existing.Iqs = row.Iqs;
        existing.HadNtrm = row.HadNtrm;
        existing.HadQualityMixture = row.HadQualityMixture;
        existing.SelfFundingPercentage = row.SelfFundingPercentage;
        existing.HaveDebt = row.HaveDebt;
        existing.Yield = row.Yield;
        existing.Scale = row.Scale;
        existing.ReforestationPercentage = row.ReforestationPercentage;
        existing.NativeForestPercentage = row.NativeForestPercentage;
        existing.NonExclusive = farmer.NonExclusiveFarmer;
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

    private static int? GetOptionalInt(CsvReader csv, string normalizedName)
    {
        var raw = GetOptionalString(csv, normalizedName);
        if (raw is null) return null;
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

    private sealed record ContractParsed(
        string FarmerCode,
        int CropSeasonId,
        string CultureTypeCode,
        int DeliveredPercentage,
        int DeliveredAmountKg,
        int ContractedAmountKg,
        int Iqs,
        bool HadNtrm,
        bool HadQualityMixture,
        int SelfFundingPercentage,
        bool HaveDebt,
        int Yield,
        int Scale,
        int ReforestationPercentage,
        int NativeForestPercentage);

    private sealed record TechnologiesParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int TechnologyId);
    private sealed record EsgIrregularityParsed(string FarmerCode, int CropSeasonId, string CultureTypeCode, int IrregularityTypeId);
}
