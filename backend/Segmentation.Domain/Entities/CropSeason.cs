namespace Segmentation.Domain.Entities;

/// <summary>
/// Crop year used across KPI facts and segmentation rules.
/// </summary>
public class CropSeason
{
    public int Id { get; set; }

    public required string Code { get; set; }
}
