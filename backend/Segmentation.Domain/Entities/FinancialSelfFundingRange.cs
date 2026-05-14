namespace Segmentation.Domain.Entities;

public class FinancialSelfFundingRange
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfigurationFinancial Financial { get; set; } = null!;

    public int Minimum { get; set; }

    public int Maximum { get; set; }

    public int CropSeasonAmount { get; set; }

    public int CropSeasonStart { get; set; }

    public int Score { get; set; }

    public ICollection<FinancialSelfFundingRangeSkippedCropSeason> SkippedCropSeasons { get; set; } =
        new List<FinancialSelfFundingRangeSkippedCropSeason>();
}
