using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiReadService
{
    Task<IReadOnlyList<LoyaltyKpiRowDto>> ListLoyaltyAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QualityKpiRowDto>> ListQualityAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialKpiRowDto>> ListFinancialAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<YieldKpiRowDto>> ListYieldAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScaleKpiRowDto>> ListScaleAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EsgKpiRowDto>> ListEsgAsync(int cropSeasonId, CancellationToken cancellationToken = default);
}
