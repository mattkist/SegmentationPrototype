namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationScale
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public ICollection<ScaleRange> Ranges { get; set; } = new List<ScaleRange>();
}
