namespace Segmentation.Domain.Entities;

/// <summary>
/// Named segment (e.g. Diamond, Gold) with score threshold and commercial discounts.
/// </summary>
public class SegmentationSegment
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public required string SegmentName { get; set; }

    public int? RangeMin { get; set; }

    public bool OnlyExclusiveFarmer { get; set; }

    public int BankDepositDiscount { get; set; }

    public int TobaccoDiscount { get; set; }

    public ICollection<SegmentationSimulationFarmer> SimulationFarmers { get; set; } = new List<SegmentationSimulationFarmer>();
    public ICollection<FarmerSegmentation> FarmerSegmentations { get; set; } = new List<FarmerSegmentation>();
}
