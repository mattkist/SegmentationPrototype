using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IFarmerReadService
{
    Task<IReadOnlyList<FarmerListItemDto>> ListForCropSeasonAsync(int cropSeasonId, CancellationToken cancellationToken = default);

    Task<FarmerDetailDto?> GetDetailAsync(Guid farmerId, int cropSeasonId, CancellationToken cancellationToken = default);

    Task<FarmerDetailDto?> GetDetailByCodeAsync(string farmerCode, int cropSeasonId, CancellationToken cancellationToken = default);
}
