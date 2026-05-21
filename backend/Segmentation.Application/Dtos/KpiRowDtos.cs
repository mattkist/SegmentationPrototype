namespace Segmentation.Application.Dtos;

public sealed record LoyaltyKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, string CultureTypeCode, int DeliveredPercentage);

public sealed record QualityKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int Iqs,
    bool HadNtrm,
    bool HadQualityMixture);

public sealed record FinancialKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int SelfFundingPercentage,
    bool HaveDebt);

public sealed record YieldKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, string CultureTypeCode, int Yield);

public sealed record ScaleKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, string CultureTypeCode, int Scale);

public sealed record TechnologiesKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    bool HasLargeBaseRidgeWithMulch,
    bool HasBroadGrateFurnace,
    bool HasTechnologyPackageAdherence,
    bool HasStandardBarn);

public sealed record EsgKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int ReforestationPercentage,
    int NativeForestPercentage,
    bool HasMinorIrregularity,
    bool HasMajorIrregularity);
