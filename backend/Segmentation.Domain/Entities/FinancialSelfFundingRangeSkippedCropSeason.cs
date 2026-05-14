namespace Segmentation.Domain.Entities;

public class FinancialSelfFundingRangeSkippedCropSeason
{
    public Guid Id { get; set; }

    public Guid FinancialSelfFundingRangeId { get; set; }
    public FinancialSelfFundingRange Range { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
