namespace Segmentation.Application.Dtos;

public sealed record LoyaltyKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, int DeliveredPercentage);

public sealed record QualityKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    int Iqs,
    bool HadNtrm,
    bool HadQualityMixture);

public sealed record FinancialKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    int SelfFundingPercentage,
    bool HaveDebt);

public sealed record YieldKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, int Yield);

public sealed record ScaleKpiRowDto(string FarmerCode, int CropSeasonId, string CropSeasonCode, int Scale);

public sealed record TechnologiesKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    bool HasLargeBaseRidgeWithMulch,
    bool HasBroadGrateFurnace,
    bool HasTechnologyPackageAdherence);

public sealed record EsgKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    int ReforestationPercentage,
    int NativeForestPercentage,
    bool HasMinorIrregularity,
    bool HasMajorIrregularity);
