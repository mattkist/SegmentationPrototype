using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SegmentationSimulationsController(ISegmentationSimulationService simulations) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] int? cropSeasonId, CancellationToken cancellationToken)
    {
        var items = await simulations.ListAsync(cropSeasonId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}", Name = "GetSegmentationSimulation")]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await simulations.GetAsync(id, cancellationToken);
        if (detail is null)
            return NotFound(new { message = "Segmentation simulation not found.", id });

        return Ok(detail);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateSegmentationSimulationDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await simulations.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetSegmentationSimulation", new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/export.csv")]
    public async Task<IActionResult> ExportCsvAsync(Guid id, CancellationToken cancellationToken)
    {
        var bytes = await simulations.ExportCsvAsync(id, cancellationToken);
        if (bytes is null)
            return NotFound(new { message = "Segmentation simulation not found.", id });

        return File(bytes, "text/csv", $"simulation-{id}.csv");
    }

    [HttpPost("{id:guid}/accept-official")]
    public async Task<IActionResult> AcceptOfficialAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await simulations.AcceptOfficialAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message, id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
