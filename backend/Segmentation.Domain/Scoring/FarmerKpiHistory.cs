namespace Segmentation.Domain.Scoring;

/// <summary>
/// KPI facts per crop season for one farmer, used when scoring a simulation window.
/// </summary>
public sealed class FarmerKpiHistory
{
    public required Guid FarmerId { get; init; }

    public bool NonExclusiveFarmer { get; init; }

    public IReadOnlyDictionary<int, int> LoyaltyDeliveredPctBySeason { get; init; } =
        new Dictionary<int, int>();

    public IReadOnlyDictionary<int, QualityKpiSnapshot> QualityBySeason { get; init; } =
        new Dictionary<int, QualityKpiSnapshot>();

    public IReadOnlyDictionary<int, FinancialKpiSnapshot> FinancialBySeason { get; init; } =
        new Dictionary<int, FinancialKpiSnapshot>();

    public IReadOnlyDictionary<int, TechnologiesKpiSnapshot> TechnologiesBySeason { get; init; } =
        new Dictionary<int, TechnologiesKpiSnapshot>();

    public IReadOnlyDictionary<int, EsgKpiSnapshot> EsgBySeason { get; init; } =
        new Dictionary<int, EsgKpiSnapshot>();

    public IReadOnlyDictionary<int, int> YieldBySeason { get; init; } =
        new Dictionary<int, int>();

    public IReadOnlyDictionary<int, int> ScaleBySeason { get; init; } =
        new Dictionary<int, int>();
}

public readonly record struct QualityKpiSnapshot(int Iqs, bool HadNtrm, bool HadQualityMixture);

public readonly record struct FinancialKpiSnapshot(int SelfFundingPercentage, bool HaveDebt);

public readonly record struct TechnologiesKpiSnapshot(
    bool HasLargeBaseRidgeWithMulch,
    bool HasBroadGrateFurnace,
    bool HasTechnologyPackageAdherence,
    bool HasStandardBarn);

public readonly record struct EsgKpiSnapshot(
    int ReforestationPercentage,
    int NativeForestPercentage,
    bool HasMinorIrregularity,
    bool HasMajorIrregularity);
