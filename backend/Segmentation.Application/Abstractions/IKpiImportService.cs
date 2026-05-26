using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiImportService
{
    Task<KpiImportResultDto> ImportFarmerContractKpisAsync(Stream csvStream, CancellationToken cancellationToken = default);

    Task<KpiImportResultDto> ImportTechnologiesAsync(Stream csvStream, CancellationToken cancellationToken = default);

    Task<KpiImportResultDto> ImportEsgIrregularitiesAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
