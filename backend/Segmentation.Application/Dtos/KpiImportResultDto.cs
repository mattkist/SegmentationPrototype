namespace Segmentation.Application.Dtos;

public sealed record KpiImportErrorDto(
    int RowNumber,
    string Message,
    string? FarmerCode = null,
    int? CropSeasonId = null);

public sealed record KpiImportResultDto(
    int TotalRows,
    int InsertedRows,
    int UpdatedRows,
    IReadOnlyList<KpiImportErrorDto> Errors);
