namespace Segmentation.Domain.Entities;

public class ScaleRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfigurationScale Scale { get; set; } = null!;

    public int Minimum { get; set; }

    public int Maximum { get; set; }

    public int CropSeasonAmount { get; set; }

    public int CropSeasonStart { get; set; }

    public int Score { get; set; }

    public ICollection<ScaleRangeSkippedCropSeason> SkippedCropSeasons { get; set; } = new List<ScaleRangeSkippedCropSeason>();
}
