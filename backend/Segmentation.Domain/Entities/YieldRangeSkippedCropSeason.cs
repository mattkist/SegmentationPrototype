namespace Segmentation.Domain.Entities;

public class YieldRangeSkippedCropSeason
{
    public Guid Id { get; set; }

    public Guid YieldRangeId { get; set; }
    public YieldRange Range { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
