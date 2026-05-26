namespace Segmentation.Domain.Scoring;

/// <summary>
/// How a single numeric KPI value is derived from multiple selected crop seasons.
/// </summary>
public static class KpiValueAggregation
{
    public const string Average = "Average";
    public const string LastActiveCropData = "LastActiveCropData";
}
