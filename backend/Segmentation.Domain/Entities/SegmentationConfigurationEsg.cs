namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationEsg
{
    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int ReforestationCropSeason { get; set; }

    public int ReforestationScorePerPercentualPoint { get; set; }

    public int ReforestationMaximumScore { get; set; }

    public int NativeForestCropSeason { get; set; }

    public int NativeForestScorePerPercentualPoint { get; set; }

    public int NativeForestMaximumScore { get; set; }

    public int MinorIrregularityCropSeason { get; set; }

    public int MinorIrregularityScore { get; set; }

    public int MajorIrregularityCropSeason { get; set; }

    public int MajorIrregularityScore { get; set; }
}
