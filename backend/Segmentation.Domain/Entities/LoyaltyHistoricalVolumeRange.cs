namespace Segmentation.Domain.Entities;

public class LoyaltyHistoricalVolumeRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationLoyalty Loyalty { get; set; } = null!;

    public int MinimumDeliveryAmount { get; set; }

    public int MaximumDeliveryAmount { get; set; }

    public int Score { get; set; }
}
