namespace Segmentation.Domain.Entities;

public class SegmentationConfiguration
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<SegmentationSegment> Segments { get; set; } = new List<SegmentationSegment>();
    public ICollection<SegmentationConfigurationCultureType> CultureTypes { get; set; } =
        new List<SegmentationConfigurationCultureType>();
    public ICollection<SegmentationSimulation> Simulations { get; set; } = new List<SegmentationSimulation>();
}
