namespace Segmentation.Domain.Entities;

/// <summary>
/// Unified contract KPI facts per farmer, crop season, and culture type.
/// Technologies and ESG irregularities remain in separate tables.
/// </summary>
public class FarmerContractKpi
{
    public Guid FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;

    public int CropSeasonId { get; set; }
    public CropSeason CropSeason { get; set; } = null!;

    public required string CultureTypeCode { get; set; }
    public CultureType CultureType { get; set; } = null!;

    public int DeliveredPercentage { get; set; }

    public int DeliveredAmountKg { get; set; }

    public int ContractedAmountKg { get; set; }

    public int Iqs { get; set; }

    public bool HadNtrm { get; set; }

    public bool HadQualityMixture { get; set; }

    public int SelfFundingPercentage { get; set; }

    public bool HaveDebt { get; set; }

    public int Yield { get; set; }

    public int Scale { get; set; }

    public int ReforestationPercentage { get; set; }

    public int NativeForestPercentage { get; set; }

    /// <summary>
    /// Snapshot of <see cref="Farmer.NonExclusiveFarmer"/> at import/load time.
    /// </summary>
    public bool NonExclusive { get; set; }
}
