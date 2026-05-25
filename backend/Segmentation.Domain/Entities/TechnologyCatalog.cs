namespace Segmentation.Domain.Entities;

public class TechnologyCatalog
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<TechnologiesKpi> TechnologiesKpis { get; set; } = new List<TechnologiesKpi>();

    public ICollection<SegmentationConfigurationTechnologyScore> ConfigurationScores { get; set; } =
        new List<SegmentationConfigurationTechnologyScore>();
}
