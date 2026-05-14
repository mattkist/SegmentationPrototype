namespace Segmentation.Domain.Entities;

public class QualityIqsRangeSkippedCropSeason
{
    public Guid Id { get; set; }

    public Guid QualityIqsRangeId { get; set; }
    public QualityIqsRange Range { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
