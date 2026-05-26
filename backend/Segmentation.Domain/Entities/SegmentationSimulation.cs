namespace Segmentation.Domain.Entities;

/// <summary>
/// One execution of a segmentation configuration.
/// Status: S = Simulation, O = Official (accepted result for <see cref="CropSeasonId"/>).
/// </summary>
public class SegmentationSimulation
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    /// <summary>Target crop season the segmentation is calculated for (official snapshot season).</summary>
    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public DateTime SimulationDate { get; set; }

    /// <summary>S = Simulation, O = Official.</summary>
    public required string Status { get; set; }

    public ICollection<SegmentationSimulationKpiScope> KpiScopes { get; set; } =
        new List<SegmentationSimulationKpiScope>();
    public ICollection<SegmentationSimulationFarmer> Farmers { get; set; } = new List<SegmentationSimulationFarmer>();
}
