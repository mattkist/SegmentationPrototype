namespace Segmentation.Application.Dtos;

public sealed class CreateSegmentationSimulationDto
{
    public Guid SegmentationConfigurationId { get; init; }
    public int CropSeasonId { get; init; }
}

public sealed class SegmentationSimulationSummaryDto
{
    public required Guid Id { get; init; }
    public required Guid SegmentationConfigurationId { get; init; }
    public required string ConfigurationName { get; init; }
    public int CropSeasonId { get; init; }
    public required string CropSeasonCode { get; init; }
    public DateTime SimulationDate { get; init; }
    public required string Status { get; init; }
    public int FarmerCount { get; init; }
}

public sealed class SegmentationSimulationFarmerDto
{
    public required Guid FarmerId { get; init; }
    public required string FarmerCode { get; init; }
    public required string FarmerName { get; init; }
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
    public int Rank { get; init; }
}

public sealed class SegmentationSimulationDetailDto
{
    public required Guid Id { get; init; }
    public required Guid SegmentationConfigurationId { get; init; }
    public required string ConfigurationName { get; init; }
    public int CropSeasonId { get; init; }
    public required string CropSeasonCode { get; init; }
    public DateTime SimulationDate { get; init; }
    public required string Status { get; init; }
    public required IReadOnlyList<SegmentationSimulationFarmerDto> Farmers { get; init; }
}
