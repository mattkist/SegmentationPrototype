using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class KpisController(
    ICropSeasonReadService cropSeasons,
    IKpiReadService kpis,
    IKpiImportService kpiImport) : ControllerBase
{
    [HttpGet("farmer-contract")]
    public Task<IActionResult> FarmerContractAsync(
        [FromQuery] int cropSeasonId,
        [FromQuery] string? cultureTypeCode,
        CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, (cs, _) => kpis.ListFarmerContractKpisAsync(cs, cultureTypeCode, cancellationToken), cancellationToken);

    [HttpGet("technologies")]
    public Task<IActionResult> TechnologiesAsync(
        [FromQuery] int cropSeasonId,
        [FromQuery] string? cultureTypeCode,
        CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, (cs, _) => kpis.ListTechnologiesAsync(cs, cultureTypeCode, cancellationToken), cancellationToken);

    [HttpGet("esg-irregularities")]
    public Task<IActionResult> EsgIrregularitiesAsync(
        [FromQuery] int cropSeasonId,
        [FromQuery] string? cultureTypeCode,
        CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, (cs, _) => kpis.ListEsgIrregularitiesAsync(cs, cultureTypeCode, cancellationToken), cancellationToken);

    [HttpPost("farmer-contract/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportFarmerContractAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportFarmerContractKpisAsync, cancellationToken);

    [HttpPost("technologies/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportTechnologiesAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportTechnologiesAsync, cancellationToken);

    [HttpPost("esg-irregularities/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportEsgIrregularitiesAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportEsgIrregularitiesAsync, cancellationToken);

    private async Task<IActionResult> GetKpiAsync<T>(
        int cropSeasonId,
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> loader,
        CancellationToken cancellationToken)
    {
        if (!await cropSeasons.ExistsAsync(cropSeasonId, cancellationToken))
            return NotFound(new { message = "Crop season not found.", cropSeasonId });

        var rows = await loader(cropSeasonId, cancellationToken);
        return Ok(rows);
    }

    private async Task<IActionResult> ImportAsync(
        IFormFile file,
        Func<Stream, CancellationToken, Task<Segmentation.Application.Dtos.KpiImportResultDto>> importer,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest(new { message = "File is empty." });

        await using var stream = file.OpenReadStream();
        var result = await importer(stream, cancellationToken);
        return Ok(result);
    }
}
