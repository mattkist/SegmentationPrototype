namespace Segmentation.Domain.Entities;

/// <summary>
/// Per-culture-type scoring rules and segment thresholds under one shared configuration header.
/// </summary>
public class SegmentationConfigurationCultureType
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public int MaximumScore { get; set; }

    public ICollection<SegmentationConfigurationCultureTypeSegment> CultureTypeSegments { get; set; } =
        new List<SegmentationConfigurationCultureTypeSegment>();

    public SegmentationConfigurationLoyalty? Loyalty { get; set; }
    public SegmentationConfigurationQuality? Quality { get; set; }
    public SegmentationConfigurationFinancial? Financial { get; set; }
    public SegmentationConfigurationTechnology? Technology { get; set; }
    public SegmentationConfigurationEsg? Esg { get; set; }
    public SegmentationConfigurationYield? Yield { get; set; }
    public SegmentationConfigurationScale? Scale { get; set; }
    public SegmentationConfigurationYieldAndScale? YieldAndScale { get; set; }
}
