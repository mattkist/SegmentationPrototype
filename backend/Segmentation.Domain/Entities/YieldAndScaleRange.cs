namespace Segmentation.Domain.Entities;

public class YieldAndScaleRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationYieldAndScale YieldAndScale { get; set; } = null!;

    public int YieldAndScaleCropSeasonAmount { get; set; }

    public int MinimumYield { get; set; }

    public int MaximumYield { get; set; }

    public decimal MinimumModule { get; set; }

    public decimal MaximumModule { get; set; }

    public int Score { get; set; }
}
