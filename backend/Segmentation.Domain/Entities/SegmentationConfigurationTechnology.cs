namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationTechnology
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public ICollection<SegmentationConfigurationTechnologyScore> TechnologyScores { get; set; } =
        new List<SegmentationConfigurationTechnologyScore>();
}
