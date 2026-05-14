namespace Segmentation.Domain.Entities;

public class SegmentationConfigurationFinancial
{
    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int MaxScore { get; set; }

    public decimal Relevance { get; set; }

    public int DebtCropSeason { get; set; }

    public int DebtScore { get; set; }

    public ICollection<FinancialSelfFundingRange> SelfFundingRanges { get; set; } = new List<FinancialSelfFundingRange>();
}
