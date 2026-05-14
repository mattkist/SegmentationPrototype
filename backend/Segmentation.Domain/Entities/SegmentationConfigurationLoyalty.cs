namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationLoyalty
{
    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public ICollection<LoyaltySeasonQuantityRange> SeasonQuantityRanges { get; set; } = new List<LoyaltySeasonQuantityRange>();
    public ICollection<LoyaltyHistoricalVolumeRange> HistoricalVolumeRanges { get; set; } = new List<LoyaltyHistoricalVolumeRange>();
}
