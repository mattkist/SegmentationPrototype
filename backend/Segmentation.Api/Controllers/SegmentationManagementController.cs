using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SegmentationManagementController(
    ICropSeasonReadService cropSeasons,
    ISegmentationManagementService management) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken)
    {
        if (!await cropSeasons.ExistsAsync(cropSeasonId, cancellationToken))
            return NotFound(new { message = "Crop season not found.", cropSeasonId });

        var rows = await management.ListAsync(cropSeasonId, cancellationToken);
        return Ok(rows);
    }

    [HttpPut("{farmerId:guid}/crop-seasons/{cropSeasonId:int}")]
    public async Task<IActionResult> SubmitForApprovalAsync(
        Guid farmerId,
        int cropSeasonId,
        [FromBody] SubmitSegmentationApprovalDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await management.SubmitForApprovalAsync(farmerId, cropSeasonId, dto, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
