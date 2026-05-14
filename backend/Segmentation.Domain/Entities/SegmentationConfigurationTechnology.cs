namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationTechnology
{
    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int HasLargeBaseRidgeWithMulchCropSeason { get; set; }

    public int HasLargeBaseRidgeWithMulchScore { get; set; }

    public int HasBroadGrateFurnaceCropSeason { get; set; }

    public int HasBroadGrateFurnaceScore { get; set; }

    public int HasTechnologyPackageAdherenceCropSeason { get; set; }

    public int HasTechnologyPackageAdherenceScore { get; set; }
}
