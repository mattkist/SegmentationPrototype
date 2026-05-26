namespace Segmentation.Domain.Scoring;

/// <summary>
/// Runtime inputs for one simulation scoring run.
/// </summary>
public sealed class SimulationScoringContext
{
    /// <summary>Target crop season (official snapshot season for the simulation run).</summary>
    public required int TargetCropSeasonId { get; init; }

    /// <summary>Per-KPI crop season selections and optional value aggregation rules.</summary>
    public required SimulationKpiScopeSet KpiScopes { get; init; }
}

/// <summary>
/// Crop seasons and aggregation mode for each KPI block in a simulation.
/// </summary>
public sealed class SimulationKpiScopeSet
{
    public required KpiScope Loyalty { get; init; }
    public required KpiScope Quality { get; init; }
    public required KpiScope Financial { get; init; }
    public required KpiScope Esg { get; init; }
    public required KpiScope Technologies { get; init; }
    public required KpiScope Yield { get; init; }
    public required KpiScope Scale { get; init; }
}

public sealed class KpiScope
{
    /// <summary>Selected seasons, newest (highest year) first.</summary>
    public required IReadOnlyList<int> CropSeasonIdsDescending { get; init; }

    /// <summary>
    /// <see cref="KpiValueAggregation.Average"/> or <see cref="KpiValueAggregation.LastActiveCropData"/>;
    /// null when the KPI always uses last active crop data only.
    /// </summary>
    public string? ValueAggregation { get; init; }

    public int LatestSeasonId =>
        CropSeasonIdsDescending.Count > 0 ? CropSeasonIdsDescending[0] : 0;
}
