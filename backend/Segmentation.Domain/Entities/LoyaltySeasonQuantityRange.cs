namespace Segmentation.Domain.Entities;

public class LoyaltySeasonQuantityRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationLoyalty Loyalty { get; set; } = null!;

    public int PlantingCropSeasonAmount { get; set; }

    public int MinimumDeliveryAmount { get; set; }

    public int MaximumDeliveryAmount { get; set; }

    public int DeliveryCropSeasonAmount { get; set; }

    public int Score { get; set; }
}
