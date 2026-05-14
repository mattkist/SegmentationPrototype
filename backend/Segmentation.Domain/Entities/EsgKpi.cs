namespace Segmentation.Domain.Entities;

public class EsgKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public int ReforestationPercentage { get; set; }

    /// <summary>
    /// Native forest percentage used by ESG scoring rules (prototype extension).
    /// </summary>
    public int NativeForestPercentage { get; set; }

    public bool HasMinorIrregularity { get; set; }

    public bool HasMajorIrregularity { get; set; }
}
