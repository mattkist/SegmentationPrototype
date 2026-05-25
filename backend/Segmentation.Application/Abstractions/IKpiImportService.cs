using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IKpiImportService
{
    Task<KpiImportResultDto> ImportLoyaltyAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportQualityAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportFinancialAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportYieldAndScaleAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportTechnologiesAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportEsgAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<KpiImportResultDto> ImportEsgIrregularitiesAsync(Stream csvStream, CancellationToken cancellationToken = default);
}
