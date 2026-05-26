using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface ISegmentationManagementService
{
    Task<IReadOnlyList<SegmentationManagementRowDto>> ListAsync(
        int cropSeasonId,
        CancellationToken cancellationToken = default);

    Task SubmitForApprovalAsync(
        Guid farmerId,
        int cropSeasonId,
        SubmitSegmentationApprovalDto dto,
        CancellationToken cancellationToken = default);
}
