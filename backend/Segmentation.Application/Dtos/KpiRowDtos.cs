namespace Segmentation.Application.Dtos;

public sealed record LoyaltyKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int DeliveredPercentage,
    int DeliveredAmountKg,
    int ContractedAmountKg);

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

public sealed record YieldAndScaleKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int Yield,
    int Scale,
    int ContractedAmountKg);

public sealed record TechnologiesKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int TechnologyId,
    string TechnologyName);

public sealed record EsgKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int ReforestationPercentage,
    int NativeForestPercentage);

public sealed record EsgIrregularityKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int IrregularityTypeId,
    string IrregularityTypeName);

public sealed record TechnologyDto(int Id, string Name);

public sealed record IrregularityTypeDto(int Id, string Name);
