namespace Segmentation.Domain.Entities;

public class LoyaltySeasonQuantityRangeSkippedCropSeason
{
    public Guid Id { get; set; }

    public Guid LoyaltySeasonQuantityRangeId { get; set; }
    public LoyaltySeasonQuantityRange Range { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
