namespace Segmentation.Domain.Entities;

public class QualityKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public int Iqs { get; set; }

    public bool HadNtrm { get; set; }

    public bool HadQualityMixture { get; set; }
}
