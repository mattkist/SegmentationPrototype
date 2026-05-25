namespace Segmentation.Domain.Entities;

public class EsgIrregularityKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public int IrregularityTypeId { get; set; }
    public IrregularityTypeCatalog IrregularityType { get; set; } = null!;
}
