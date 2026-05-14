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
    [HttpGet("loyalty")]
    public Task<IActionResult> LoyaltyAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListLoyaltyAsync, cancellationToken);

    [HttpGet("quality")]
    public Task<IActionResult> QualityAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListQualityAsync, cancellationToken);

    [HttpGet("financial")]
    public Task<IActionResult> FinancialAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListFinancialAsync, cancellationToken);

    [HttpGet("yield")]
    public Task<IActionResult> YieldAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListYieldAsync, cancellationToken);

    [HttpGet("scale")]
    public Task<IActionResult> ScaleAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListScaleAsync, cancellationToken);

    [HttpGet("technologies")]
    public Task<IActionResult> TechnologiesAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListTechnologiesAsync, cancellationToken);

    [HttpGet("esg")]
    public Task<IActionResult> EsgAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken) =>
        GetKpiAsync(cropSeasonId, kpis.ListEsgAsync, cancellationToken);

    [HttpPost("loyalty/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportLoyaltyAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportLoyaltyAsync, cancellationToken);

    [HttpPost("quality/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportQualityAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportQualityAsync, cancellationToken);

    [HttpPost("financial/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportFinancialAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportFinancialAsync, cancellationToken);

    [HttpPost("yield/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportYieldAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportYieldAsync, cancellationToken);

    [HttpPost("scale/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportScaleAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportScaleAsync, cancellationToken);

    [HttpPost("technologies/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportTechnologiesAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportTechnologiesAsync, cancellationToken);

    [HttpPost("esg/import")]
    [Consumes("multipart/form-data")]
    public Task<IActionResult> ImportEsgAsync([FromForm] IFormFile file, CancellationToken cancellationToken) =>
        ImportAsync(file, kpiImport.ImportEsgAsync, cancellationToken);

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
