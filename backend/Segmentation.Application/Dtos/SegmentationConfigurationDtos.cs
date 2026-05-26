namespace Segmentation.Application.Dtos;

public sealed class SegmentationConfigurationSummaryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<string> CultureTypeCodes { get; init; }
}

public sealed class SegmentationConfigurationDetailDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<SegmentationSegmentDto> Segments { get; init; }
    public required IReadOnlyList<CultureTypeConfigurationDetailDto> CultureTypes { get; init; }
}

public sealed class SegmentationSegmentDto
{
    public Guid? Id { get; init; }
    public required string SegmentName { get; init; }
    public bool OnlyExclusiveFarmer { get; init; }
    public int BankDepositDiscount { get; init; }
    public int TobaccoDiscount { get; init; }
}

public sealed class CultureTypeSegmentThresholdDto
{
    public Guid? SegmentId { get; init; }
    public required string SegmentName { get; init; }
    public int? RangeMin { get; init; }
}

public sealed class CultureTypeConfigurationDetailDto
{
    public Guid? Id { get; init; }
    public required string CultureTypeCode { get; init; }
    public int MaximumScore { get; init; }
    public required IReadOnlyList<CultureTypeSegmentThresholdDto> SegmentThresholds { get; init; }
    public required SegmentationLoyaltyDetailDto Loyalty { get; init; }
    public required SegmentationQualityDetailDto Quality { get; init; }
    public required SegmentationFinancialDetailDto Financial { get; init; }
    public required SegmentationTechnologyDetailDto Technology { get; init; }
    public required SegmentationEsgDetailDto Esg { get; init; }
    public required SegmentationYieldDetailDto Yield { get; init; }
    public required SegmentationScaleDetailDto Scale { get; init; }
}

public sealed class SegmentationLoyaltyDetailDto : SegmentationLoyaltyWriteDto;

public class SegmentationLoyaltyWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public required IReadOnlyList<LoyaltySeasonQuantityRangeDto> SeasonQuantityRanges { get; init; }
    public required IReadOnlyList<LoyaltyHistoricalVolumeRangeDto> HistoricalVolumeRanges { get; init; }
}

public sealed class LoyaltySeasonQuantityRangeDto
{
    public int PlantingCropSeasonAmount { get; init; }
    public int MinimumDeliveryAmount { get; init; }
    public int MaximumDeliveryAmount { get; init; }
    public int DeliveryCropSeasonAmount { get; init; }
    public int Score { get; init; }
}

public sealed class LoyaltyHistoricalVolumeRangeDto
{
    public int MinimumDeliveryAmount { get; init; }
    public int MaximumDeliveryAmount { get; init; }
    public int Score { get; init; }
}

public sealed class SegmentationQualityDetailDto : SegmentationQualityWriteDto;

public class SegmentationQualityWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public int NtrmScore { get; init; }
    public int MixtureScore { get; init; }
    public required IReadOnlyList<QualityIqsRangeDto> IqsRanges { get; init; }
}

public sealed class QualityIqsRangeDto
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int CropSeasonAmount { get; init; }
    public int Score { get; init; }
}

public sealed class SegmentationFinancialDetailDto : SegmentationFinancialWriteDto;

public class SegmentationFinancialWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public int DebtScore { get; init; }
    public required IReadOnlyList<FinancialSelfFundingRangeDto> SelfFundingRanges { get; init; }
}

public sealed class FinancialSelfFundingRangeDto
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int CropSeasonAmount { get; init; }
    public int Score { get; init; }
}

public sealed class SegmentationTechnologyDetailDto : SegmentationTechnologyWriteDto;

public sealed class TechnologyScoreDto
{
    public int TechnologyId { get; init; }
    public int Score { get; init; }
}

public class SegmentationTechnologyWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public required IReadOnlyList<TechnologyScoreDto> TechnologyScores { get; init; }
}

public sealed class SegmentationEsgDetailDto : SegmentationEsgWriteDto;

public sealed class EsgIrregularityScoreDto
{
    public int IrregularityTypeId { get; init; }
    public int Score { get; init; }
}

public class SegmentationEsgWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public int ReforestationScorePerPercentualPoint { get; init; }
    public int ReforestationMaximumScore { get; init; }
    public int NativeForestScorePerPercentualPoint { get; init; }
    public int NativeForestMaximumScore { get; init; }
    public required IReadOnlyList<EsgIrregularityScoreDto> IrregularityScores { get; init; }
}

public sealed class SegmentationYieldDetailDto : SegmentationYieldWriteDto;

public class SegmentationYieldWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public required IReadOnlyList<YieldRangeDto> Ranges { get; init; }
}

public sealed class YieldRangeDto
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int CropSeasonAmount { get; init; }
    public int Score { get; init; }
}

public sealed class SegmentationScaleDetailDto : SegmentationScaleWriteDto;

public class SegmentationScaleWriteDto
{
    public int MaxScore { get; init; }
    public decimal Relevance { get; init; }
    public required IReadOnlyList<ScaleRangeDto> Ranges { get; init; }
}

public sealed class ScaleRangeDto
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int CropSeasonAmount { get; init; }
    public int Score { get; init; }
}

public sealed class CultureTypeConfigurationWriteDto
{
    public Guid? Id { get; init; }
    public required string CultureTypeCode { get; init; }
    public int MaximumScore { get; init; }
    public required IReadOnlyList<CultureTypeSegmentThresholdDto> SegmentThresholds { get; init; }
    public required SegmentationLoyaltyWriteDto Loyalty { get; init; }
    public required SegmentationQualityWriteDto Quality { get; init; }
    public required SegmentationFinancialWriteDto Financial { get; init; }
    public required SegmentationTechnologyWriteDto Technology { get; init; }
    public required SegmentationEsgWriteDto Esg { get; init; }
    public required SegmentationYieldWriteDto Yield { get; init; }
    public required SegmentationScaleWriteDto Scale { get; init; }
}

public sealed class SaveSegmentationConfigurationDto
{
    public required string Name { get; init; }
    public required IReadOnlyList<SegmentationSegmentDto> Segments { get; init; }
    public required IReadOnlyList<CultureTypeConfigurationWriteDto> CultureTypes { get; init; }
}

public sealed class DuplicateSegmentationConfigurationRequest
{
    public string? Name { get; init; }
}

public sealed class PatchSegmentationConfigurationNameDto
{
    public required string Name { get; init; }
}
