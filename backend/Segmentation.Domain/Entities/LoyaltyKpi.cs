namespace Segmentation.Domain.Entities;

public class LoyaltyKpi
{
    public Guid Id { get; set; }

    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    /// <summary>
    /// Delivered percentage for the farmer in this crop season (0–100 scale per prototype seed).
    /// </summary>
    public int DeliveredPercentage { get; set; }
}
