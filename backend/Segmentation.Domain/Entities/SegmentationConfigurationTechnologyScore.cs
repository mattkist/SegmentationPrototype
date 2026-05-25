namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationTechnologyScore
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationTechnology TechnologyBlock { get; set; } = null!;

    public int TechnologyId { get; set; }
    public TechnologyCatalog Technology { get; set; } = null!;

    public int Score { get; set; }
}
