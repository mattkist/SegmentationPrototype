namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationEsg
{
    public Guid SegmentationConfigurationCultureTypeId { get; set; }
    public SegmentationConfigurationCultureType CultureType { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int ReforestationScorePerPercentualPoint { get; set; }

    public int ReforestationMaximumScore { get; set; }

    public int NativeForestScorePerPercentualPoint { get; set; }

    public int NativeForestMaximumScore { get; set; }

    public int MinorIrregularityScore { get; set; }

    public int MajorIrregularityScore { get; set; }
}
