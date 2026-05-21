namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationYieldAndScale
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public ICollection<YieldAndScaleRange> Ranges { get; set; } = new List<YieldAndScaleRange>();
}
