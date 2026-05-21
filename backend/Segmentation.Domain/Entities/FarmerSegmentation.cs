namespace Segmentation.Domain.Entities;

/// <summary>
/// Official segmentation snapshot per farmer and crop season (fed when a simulation is accepted).
/// </summary>
public class FarmerSegmentation
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public int TotalScore { get; set; }

    public int LoyaltyScore { get; set; }

    public int QualityScore { get; set; }

    public int FinancialScore { get; set; }

    public int TechnologiesScore { get; set; }

    public int EsgScore { get; set; }

    public int YieldScore { get; set; }

    public int ScaleScore { get; set; }

    public int YieldAndScaleScore { get; set; }

    public bool NonExclusiveFarmer { get; set; }

    public Guid? SegmentationConfigurationSegmentId { get; set; }
    public SegmentationSegment? Segment { get; set; }

    public int Rank { get; set; }
}
