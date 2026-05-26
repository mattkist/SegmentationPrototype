using Segmentation.Application.Dtos;

namespace Segmentation.Application.Abstractions;

public interface IReferenceDataReadService
{
    Task<IReadOnlyList<TechnologyDto>> ListTechnologiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IrregularityTypeDto>> ListIrregularityTypesAsync(CancellationToken cancellationToken = default);
}
