using Segmentation.Domain.Entities;

namespace Segmentation.Domain.Scoring;

/// <summary>
/// Culture-type KPI rules and segment thresholds used during simulation scoring.
/// </summary>
public sealed class CultureTypeScoringBundle
{
    public required Guid CultureTypeConfigurationId { get; init; }
    public required string CultureTypeCode { get; init; }
    public required SegmentationConfigurationLoyalty Loyalty { get; init; }
    public required SegmentationConfigurationQuality Quality { get; init; }
    public required SegmentationConfigurationFinancial Financial { get; init; }
    public required SegmentationConfigurationTechnology Technology { get; init; }
    public required SegmentationConfigurationEsg Esg { get; init; }
    public required SegmentationConfigurationYield Yield { get; init; }
    public required SegmentationConfigurationScale Scale { get; init; }
    public required SegmentationConfigurationYieldAndScale YieldAndScale { get; init; }
    public required IReadOnlyList<SegmentThreshold> SegmentThresholds { get; init; }
}

public readonly record struct SegmentThreshold(Guid SegmentId, int? RangeMin, bool OnlyExclusiveFarmer);
