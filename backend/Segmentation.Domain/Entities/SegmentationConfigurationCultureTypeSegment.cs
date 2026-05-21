namespace Segmentation.Domain.Entities;

/// <summary>
/// Culture-type-specific score threshold for a header segment definition.
/// </summary>
public class SegmentationConfigurationCultureTypeSegment
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public Guid SegmentationSegmentId { get; set; }
    public SegmentationSegment Segment { get; set; } = null!;

    public int? RangeMin { get; set; }
}
