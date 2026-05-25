namespace Segmentation.Domain.Entities;

public class YieldAndScaleKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public int Yield { get; set; }

    public int Scale { get; set; }

    public int ContractedAmountKg { get; set; }
}
