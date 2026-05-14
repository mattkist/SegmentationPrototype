namespace Segmentation.Domain.Entities;

public class ScaleRangeSkippedCropSeason
{
    public Guid Id { get; set; }

    public Guid ScaleRangeId { get; set; }
    public ScaleRange Range { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
