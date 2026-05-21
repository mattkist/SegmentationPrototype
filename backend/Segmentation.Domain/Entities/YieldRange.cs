namespace Segmentation.Domain.Entities;

public class YieldRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationYield Yield { get; set; } = null!;

    public int Minimum { get; set; }

    public int Maximum { get; set; }

    public int CropSeasonAmount { get; set; }

    public int Score { get; set; }
}
