namespace Segmentation.Domain.Entities;

/// <summary>
/// Named segment (e.g. Diamond, Gold) with commercial discounts. Score thresholds are per culture type.
/// </summary>
public class SegmentationSegment
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public required string SegmentName { get; set; }

    public bool OnlyExclusiveFarmer { get; set; }

    public int BankDepositDiscount { get; set; }

    public int TobaccoDiscount { get; set; }

    public ICollection<SegmentationConfigurationCultureTypeSegment> CultureTypeSegments { get; set; } =
        new List<SegmentationConfigurationCultureTypeSegment>();
    public ICollection<SegmentationSimulationFarmer> SimulationFarmers { get; set; } = new List<SegmentationSimulationFarmer>();
    public ICollection<FarmerSegmentation> FarmerSegmentations { get; set; } = new List<FarmerSegmentation>();
}
