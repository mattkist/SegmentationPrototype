namespace Segmentation.Domain.Entities;

public class TechnologiesKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public bool HasLargeBaseRidgeWithMulch { get; set; }

    public bool HasBroadGrateFurnace { get; set; }

    public bool HasTechnologyPackageAdherence { get; set; }

    public bool HasStandardBarn { get; set; }
}
