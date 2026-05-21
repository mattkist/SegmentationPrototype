namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationTechnology
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int HasLargeBaseRidgeWithMulchScore { get; set; }

    public int HasBroadGrateFurnaceScore { get; set; }

    public int HasTechnologyPackageAdherenceScore { get; set; }

    public int HasStandardBarnScore { get; set; }
}
