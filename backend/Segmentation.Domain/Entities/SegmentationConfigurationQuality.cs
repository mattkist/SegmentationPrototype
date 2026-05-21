namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationQuality
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int NtrmScore { get; set; }

    public int MixtureScore { get; set; }

    public ICollection<QualityIqsRange> IqsRanges { get; set; } = new List<QualityIqsRange>();
}
