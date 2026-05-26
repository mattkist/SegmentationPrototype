using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiReadService
{
    Task<IReadOnlyList<FarmerContractKpiRowDto>> ListFarmerContractKpisAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TechnologiesKpiRowDto>> ListTechnologiesAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EsgIrregularityKpiRowDto>> ListEsgIrregularitiesAsync(
        int cropSeasonId,
        string? cultureTypeCode = null,
        CancellationToken cancellationToken = default);
}
