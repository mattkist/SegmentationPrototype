namespace Segmentation.Domain.Entities;

public class IrregularityTypeCatalog
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<EsgIrregularityKpi> EsgIrregularityKpis { get; set; } = new List<EsgIrregularityKpi>();

    public ICollection<SegmentationConfigurationEsgIrregularityScore> ConfigurationScores { get; set; } =
        new List<SegmentationConfigurationEsgIrregularityScore>();
}
