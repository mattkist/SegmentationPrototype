namespace Segmentation.Application.Dtos;

public sealed record FarmerDetailDto(
    Guid FarmerId,
    string FarmerCode,
    string FarmerName,
    bool NonExclusiveFarmer,
    int CropSeasonId,
    string CropSeasonCode,
    OfficialSegmentationDto? OfficialSegmentation,
    FarmerKpisForSeasonDto Kpis);

public sealed record OfficialSegmentationDto(
    int TotalScore,
    int Rank,
    string? SegmentName,
    Guid? SegmentationConfigurationSegmentId,
    int LoyaltyScore,
    int QualityScore,
    int FinancialScore,
    int TechnologiesScore,
    int EsgScore,
    int YieldScore,
    int ScaleScore,
    int YieldAndScaleScore);

public sealed record FarmerKpisForSeasonDto(
    LoyaltyKpiRowDto? Loyalty,
    QualityKpiRowDto? Quality,
    FinancialKpiRowDto? Financial,
    YieldAndScaleKpiRowDto? YieldAndScale,
    IReadOnlyList<TechnologiesKpiRowDto> Technologies,
    EsgKpiRowDto? Esg,
    IReadOnlyList<EsgIrregularityKpiRowDto> EsgIrregularities);
