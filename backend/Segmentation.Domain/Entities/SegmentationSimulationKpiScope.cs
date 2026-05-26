namespace Segmentation.Domain.Entities;

/// <summary>
/// Per-KPI crop season selection and optional value aggregation for a simulation run.
/// </summary>
public class SegmentationSimulationKpiScope
{
    public Guid Id { get; set; }

    public Guid SegmentationSimulationId { get; set; }
    public SegmentationSimulation SegmentationSimulation { get; set; } = null!;

    /// <summary>See <see cref="Scoring.SimulationKpiKind"/>.</summary>
    public required string KpiKind { get; set; }

    /// <summary>
    /// <see cref="Scoring.KpiValueAggregation.Average"/> or
    /// <see cref="Scoring.KpiValueAggregation.LastActiveCropData"/>; null when not applicable.
    /// </summary>
    public string? ValueAggregation { get; set; }

    public ICollection<SegmentationSimulationKpiScopeSeason> Seasons { get; set; } =
        new List<SegmentationSimulationKpiScopeSeason>();
}

public class SegmentationSimulationKpiScopeSeason
{
    public Guid Id { get; set; }

    public Guid SegmentationSimulationKpiScopeId { get; set; }
    public SegmentationSimulationKpiScope KpiScope { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;
}
