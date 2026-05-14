using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface ISegmentationConfigurationService
{
    Task<IReadOnlyList<SegmentationConfigurationSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<SegmentationConfigurationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SegmentationConfigurationDetailDto> CreateAsync(
        SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken = default);

    Task<SegmentationConfigurationDetailDto> UpdateAsync(
        Guid id,
        SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken = default);

    Task<SegmentationConfigurationDetailDto> PatchNameAsync(
        Guid id,
        string name,
        CancellationToken cancellationToken = default);

    Task<SegmentationConfigurationDetailDto> DuplicateAsync(
        Guid id,
        string? newName,
        CancellationToken cancellationToken = default);
}
