namespace Segmentation.Application.Dtos;

public sealed record FarmerContractKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
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
    int NativeForestPercentage,
    bool NonExclusive);

public sealed record TechnologiesKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int TechnologyId,
    string TechnologyName);

public sealed record EsgIrregularityKpiRowDto(
    string FarmerCode,
    int CropSeasonId,
    string CropSeasonCode,
    string CultureTypeCode,
    int IrregularityTypeId,
    string IrregularityTypeName);

public sealed record TechnologyDto(int Id, string Name);

public sealed record IrregularityTypeDto(int Id, string Name);
