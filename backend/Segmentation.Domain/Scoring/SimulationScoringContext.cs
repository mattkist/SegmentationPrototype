namespace Segmentation.Domain.Scoring;

/// <summary>
/// Runtime inputs for one simulation scoring run.
/// </summary>
public sealed class SimulationScoringContext
{
    /// <summary>Target crop season (official snapshot season for the simulation run).</summary>
    public required int TargetCropSeasonId { get; init; }

    /// <summary>Selected crop seasons for multi-season rules, newest first.</summary>
    public required IReadOnlyList<int> ScopeCropSeasonIdsDescending { get; init; }

    /// <summary>
    /// Highest crop season id in <see cref="ScopeCropSeasonIdsDescending"/> (latest year in scope).
    /// Non-loyalty KPIs read facts from this season only.
    /// </summary>
    public int LatestScopeCropSeasonId =>
        ScopeCropSeasonIdsDescending.Count > 0 ? ScopeCropSeasonIdsDescending[0] : TargetCropSeasonId;
}
