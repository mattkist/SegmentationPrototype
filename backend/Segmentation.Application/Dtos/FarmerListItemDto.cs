namespace Segmentation.Application.Dtos;

/// <summary>
/// Farmer row for a crop season list, including official segmentation when present.
/// </summary>
public sealed record FarmerListItemDto(
    Guid FarmerId,
    string FarmerCode,
    string FarmerName,
    bool NonExclusiveFarmer,
    int? TotalScore,
    int? Rank,
    string? SegmentName,
    Guid? SegmentationConfigurationSegmentId);
