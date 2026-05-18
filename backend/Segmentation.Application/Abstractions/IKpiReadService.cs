using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiReadService
{
    Task<IReadOnlyList<LoyaltyKpiRowDto>> ListLoyaltyAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QualityKpiRowDto>> ListQualityAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialKpiRowDto>> ListFinancialAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<YieldKpiRowDto>> ListYieldAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScaleKpiRowDto>> ListScaleAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EsgKpiRowDto>> ListEsgAsync(int cropSeasonId, string? cultureTypeCode = null, CancellationToken cancellationToken = default);
}
