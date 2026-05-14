using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface ICropSeasonReadService
{
    Task<IReadOnlyList<CropSeasonDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int cropSeasonId, CancellationToken cancellationToken = default);
}
