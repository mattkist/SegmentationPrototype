namespace Segmentation.Domain.Entities;

public class Farmer
{
    public Guid Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    /// <summary>
    /// When true, the farmer is non-exclusive (shared / multi-channel context per agronomy rules).
    /// </summary>
    public bool NonExclusiveFarmer { get; set; }

    public ICollection<FarmerClusterFarmer> ClusterLinks { get; set; } = new List<FarmerClusterFarmer>();
    public ICollection<LoyaltyKpi> LoyaltyKpis { get; set; } = new List<LoyaltyKpi>();
    public ICollection<QualityKpi> QualityKpis { get; set; } = new List<QualityKpi>();
    public ICollection<FinancialKpi> FinancialKpis { get; set; } = new List<FinancialKpi>();
    public ICollection<YieldAndScaleKpi> YieldAndScaleKpis { get; set; } = new List<YieldAndScaleKpi>();
    public ICollection<TechnologiesKpi> TechnologiesKpis { get; set; } = new List<TechnologiesKpi>();
    public ICollection<EsgKpi> EsgKpis { get; set; } = new List<EsgKpi>();
    public ICollection<EsgIrregularityKpi> EsgIrregularityKpis { get; set; } = new List<EsgIrregularityKpi>();
    public ICollection<SegmentationSimulationFarmer> SimulationResults { get; set; } = new List<SegmentationSimulationFarmer>();
    public ICollection<FarmerSegmentation> Segmentations { get; set; } = new List<FarmerSegmentation>();
}
