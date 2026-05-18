namespace Segmentation.Domain.Entities;

public class ScaleKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    /// <summary>
    /// Area / scale index for the prototype (integer units).
    /// </summary>
    public int Scale { get; set; }
}
