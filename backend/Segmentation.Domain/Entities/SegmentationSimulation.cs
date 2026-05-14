namespace Segmentation.Domain.Entities;

/// <summary>
/// One execution of a segmentation configuration for a crop season.
/// Status: S = Simulation, O = Official (accepted result for that crop season).
/// </summary>
public class SegmentationSimulation
{
    public Guid Id { get; set; }

    public Guid SegmentationConfigurationId { get; set; }
    public SegmentationConfiguration SegmentationConfiguration { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public DateTime SimulationDate { get; set; }

    /// <summary>S = Simulation, O = Official.</summary>
    public required string Status { get; set; }

    public ICollection<SegmentationSimulationFarmer> Farmers { get; set; } = new List<SegmentationSimulationFarmer>();
}
