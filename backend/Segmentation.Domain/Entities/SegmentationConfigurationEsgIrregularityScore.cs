namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationEsgIrregularityScore
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationEsg EsgBlock { get; set; } = null!;

    public int IrregularityTypeId { get; set; }
    public IrregularityTypeCatalog IrregularityType { get; set; } = null!;

    public int Score { get; set; }
}
