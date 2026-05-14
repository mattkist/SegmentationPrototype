namespace Segmentation.Domain.Entities;

public class YieldKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public int Yield { get; set; }
}
