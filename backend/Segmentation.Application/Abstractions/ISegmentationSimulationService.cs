using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface ISegmentationSimulationService
{
    Task<SegmentationSimulationDetailDto> CreateAsync(
        CreateSegmentationSimulationDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SegmentationSimulationSummaryDto>> ListAsync(
        int? cropSeasonId,
        CancellationToken cancellationToken = default);

    Task<SegmentationSimulationDetailDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task AcceptOfficialAsync(Guid simulationId, CancellationToken cancellationToken = default);

    Task<byte[]?> ExportCsvAsync(Guid id, CancellationToken cancellationToken = default);
}
