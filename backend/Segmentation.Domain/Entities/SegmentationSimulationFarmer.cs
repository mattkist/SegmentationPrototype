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

    public bool NonExclusiveFarmer { get; set; }

    public required string CultureTypeCode { get; set; }

    public Guid? SegmentationConfigurationSegmentId { get; set; }
    public SegmentationSegment? Segment { get; set; }

    /// <summary>Legacy column; rankings are no longer computed (always 0).</summary>
    public int Rank { get; set; }

    /// <summary>True when the farmer had no contract in the season before the simulation target season.</summary>
    public bool IsNewFarmer { get; set; }
}
