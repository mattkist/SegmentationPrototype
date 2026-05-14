namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationQuality
{
    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int NtrmCropSeasonAmount { get; set; }

    public int NtrmCropSeasonStart { get; set; }

    public int NtrmScore { get; set; }

    public int MixtureCropSeasonAmount { get; set; }

    public int MixtureCropSeasonStart { get; set; }

    public int MixtureScore { get; set; }

    public ICollection<QualityIqsRange> IqsRanges { get; set; } = new List<QualityIqsRange>();
}
