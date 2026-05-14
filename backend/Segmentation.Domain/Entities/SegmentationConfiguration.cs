namespace Segmentation.Domain.Entities;

public class SegmentationConfiguration
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public int MaximumScore { get; set; }

    public ICollection<SegmentationSegment> Segments { get; set; } = new List<SegmentationSegment>();
    public SegmentationConfigurationLoyalty? Loyalty { get; set; }
    public SegmentationConfigurationQuality? Quality { get; set; }
    public SegmentationConfigurationFinancial? Financial { get; set; }
    public SegmentationConfigurationTechnology? Technology { get; set; }
    public SegmentationConfigurationEsg? Esg { get; set; }
    public SegmentationConfigurationYield? Yield { get; set; }
    public SegmentationConfigurationScale? Scale { get; set; }
    public ICollection<SegmentationSimulation> Simulations { get; set; } = new List<SegmentationSimulation>();
}
