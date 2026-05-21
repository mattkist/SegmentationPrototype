namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationYield
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public ICollection<YieldRange> Ranges { get; set; } = new List<YieldRange>();
}
