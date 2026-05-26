namespace Segmentation.Domain.Scoring;

public static class SimulationKpiKind
{
    public const string Loyalty = "Loyalty";
    public const string Quality = "Quality";
    public const string Financial = "Financial";
    public const string Esg = "Esg";
    public const string Technologies = "Technologies";
    public const string Yield = "Yield";
    public const string Scale = "Scale";

    public static readonly IReadOnlyList<string> All =
    [
        Loyalty,
        Quality,
        Financial,
        Esg,
        Technologies,
        Yield,
        Scale
    ];
}
