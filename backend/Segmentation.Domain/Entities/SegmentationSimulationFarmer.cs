namespace Segmentation.Domain.Entities;

public class SegmentationSimulationFarmer
{
    public Guid Id { get; set; }

    public Guid SegmentationSimulationId { get; set; }
    public SegmentationSimulation SegmentationSimulation { get; set; } = null!;

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

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

    public required string CultureTypeCode { get; set; }

    public Guid? SegmentationConfigurationSegmentId { get; set; }
    public SegmentationSegment? Segment { get; set; }

    /// <summary>Legacy column; rankings are no longer computed (always 0).</summary>
    public int Rank { get; set; }
}
