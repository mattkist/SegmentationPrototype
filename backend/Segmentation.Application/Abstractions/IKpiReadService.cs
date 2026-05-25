using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiReadService
{
    Task<IReadOnlyList<LoyaltyKpiRowDto>> ListLoyaltyAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QualityKpiRowDto>> ListQualityAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialKpiRowDto>> ListFinancialAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<YieldAndScaleKpiRowDto>> ListYieldAndScaleAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EsgKpiRowDto>> ListEsgAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EsgIrregularityKpiRowDto>> ListEsgIrregularitiesAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);
}

public interface IReferenceDataReadService
{
    Task<IReadOnlyList<TechnologyDto>> ListTechnologiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IrregularityTypeDto>> ListIrregularityTypesAsync(CancellationToken cancellationToken = default);
}
