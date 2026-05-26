namespace Segmentation.Application.Dtos;

public sealed class SimulationKpiScopeInputDto
{
    public required string KpiKind { get; init; }
    public required IReadOnlyList<int> CropSeasonIds { get; init; }
    public string? ValueAggregation { get; init; }
}

public sealed class CreateSegmentationSimulationDto
{
    public Guid SegmentationConfigurationId { get; init; }

    /// <summary>Target crop season for the segmentation (official snapshot season).</summary>
    public int CropSeasonId { get; init; }

    /// <summary>Per-KPI crop season selections and optional value aggregation rules.</summary>
    public required IReadOnlyList<SimulationKpiScopeInputDto> KpiScopes { get; init; }
}

public sealed class SegmentationSimulationSummaryDto
{
    public required Guid Id { get; init; }
    public required Guid SegmentationConfigurationId { get; init; }
    public required string ConfigurationName { get; init; }
    public int CropSeasonId { get; init; }
    public required string CropSeasonCode { get; init; }
    public required IReadOnlyList<SimulationKpiScopeInputDto> KpiScopes { get; init; }
    public DateTime SimulationDate { get; init; }
    public required string Status { get; init; }
    public int FarmerCount { get; init; }
}

public sealed class SegmentationSimulationFarmerDto
{
    public required Guid FarmerId { get; init; }
    public required string FarmerCode { get; init; }
    public required string FarmerName { get; init; }
    public required string CultureTypeCode { get; init; }
    public int TotalScore { get; init; }
    public int LoyaltyScore { get; init; }
    public int QualityScore { get; init; }
    public int FinancialScore { get; init; }
    public int TechnologiesScore { get; init; }
    public int EsgScore { get; init; }
    public int YieldScore { get; init; }
    public int ScaleScore { get; init; }
    public bool NonExclusiveFarmer { get; init; }
    public Guid? SegmentationConfigurationSegmentId { get; init; }
    public string? SegmentName { get; init; }
    public bool IsNewFarmer { get; init; }
}

public sealed class SegmentShareDto
{
    public required string SegmentName { get; init; }
    public int FarmerCount { get; init; }
    public decimal Percentage { get; init; }
}

public sealed class CultureTypeSegmentDistributionDto
{
    public required string CultureTypeCode { get; init; }
    public required IReadOnlyList<SegmentShareDto> Segments { get; init; }
}

public sealed class SegmentationSimulationDetailDto
{
    public required Guid Id { get; init; }
    public required Guid SegmentationConfigurationId { get; init; }
    public required string ConfigurationName { get; init; }
    public int CropSeasonId { get; init; }
    public required string CropSeasonCode { get; init; }
    public required IReadOnlyList<SimulationKpiScopeInputDto> KpiScopes { get; init; }
    public DateTime SimulationDate { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<SegmentationSimulationFarmerDto> Farmers { get; init; }
    public required IReadOnlyList<SegmentShareDto> OverallSegmentDistribution { get; init; }
    public required IReadOnlyList<CultureTypeSegmentDistributionDto> SegmentDistributionByCultureType { get; init; }
}

public sealed class SegmentationManagementRowDto
{
    public required Guid FarmerId { get; init; }
    public required string FarmerCode { get; init; }
    public required string FarmerName { get; init; }
    public required string CultureTypeCode { get; init; }
    public int TotalScore { get; init; }
    public Guid? SegmentationConfigurationSegmentId { get; init; }
    public string? SegmentName { get; init; }
    public required Guid SegmentationConfigurationId { get; init; }
    public required IReadOnlyList<SegmentationSegmentDto> AvailableSegments { get; init; }
}

public sealed class SubmitSegmentationApprovalDto
{
    public Guid? SegmentationConfigurationSegmentId { get; init; }
}
