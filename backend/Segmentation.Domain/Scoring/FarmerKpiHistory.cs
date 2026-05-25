namespace Segmentation.Domain.Scoring;

/// <summary>
/// KPI facts per crop season for one farmer, used when scoring a simulation window.
/// </summary>
public sealed class FarmerKpiHistory
{
    public required Guid FarmerId { get; init; }

    public bool NonExclusiveFarmer { get; init; }

    public IReadOnlyDictionary<int, LoyaltyKpiSnapshot> LoyaltyBySeason { get; init; } =
        new Dictionary<int, LoyaltyKpiSnapshot>();

    public IReadOnlyDictionary<int, QualityKpiSnapshot> QualityBySeason { get; init; } =
        new Dictionary<int, QualityKpiSnapshot>();

    public IReadOnlyDictionary<int, FinancialKpiSnapshot> FinancialBySeason { get; init; } =
        new Dictionary<int, FinancialKpiSnapshot>();

    public IReadOnlyDictionary<int, TechnologiesKpiSnapshot> TechnologiesBySeason { get; init; } =
        new Dictionary<int, TechnologiesKpiSnapshot>();

    public IReadOnlyDictionary<int, EsgKpiSnapshot> EsgBySeason { get; init; } =
        new Dictionary<int, EsgKpiSnapshot>();

    public IReadOnlyDictionary<int, YieldAndScaleKpiSnapshot> YieldAndScaleBySeason { get; init; } =
        new Dictionary<int, YieldAndScaleKpiSnapshot>();
}

public readonly record struct LoyaltyKpiSnapshot(int DeliveredAmountKg, int ContractedAmountKg);

public readonly record struct QualityKpiSnapshot(int Iqs, bool HadNtrm, bool HadQualityMixture);

public readonly record struct FinancialKpiSnapshot(int SelfFundingPercentage, bool HaveDebt);

public readonly record struct TechnologiesKpiSnapshot(HashSet<int> TechnologyIds);

public readonly record struct EsgKpiSnapshot(
    int ReforestationPercentage,
    int NativeForestPercentage,
    HashSet<int> IrregularityTypeIds);

public readonly record struct YieldAndScaleKpiSnapshot(int Yield, int Scale, int ContractedAmountKg);
