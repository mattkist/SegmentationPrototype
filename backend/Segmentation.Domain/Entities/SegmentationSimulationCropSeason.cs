namespace Segmentation.Domain.Entities;

/// <summary>
/// One crop season included in a simulation's scoring scope (historical window for rules).
/// </summary>
public class SegmentationSimulationCropSeason
{
    public Guid Id { get; set; }

    public Guid SegmentationSimulationId { get; set; }
    public SegmentationSimulation SegmentationSimulation { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
