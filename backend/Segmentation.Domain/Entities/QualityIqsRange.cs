namespace Segmentation.Domain.Entities;

public class QualityIqsRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfigurationQuality Quality { get; set; } = null!;

    public int Minimum { get; set; }

    public int Maximum { get; set; }

    public int CropSeasonAmount { get; set; }

    public int CropSeasonStart { get; set; }

    public int Score { get; set; }

    public ICollection<QualityIqsRangeSkippedCropSeason> SkippedCropSeasons { get; set; } =
        new List<QualityIqsRangeSkippedCropSeason>();
}
