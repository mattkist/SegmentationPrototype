using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FarmersController(ICropSeasonReadService cropSeasons, IFarmerReadService farmers) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] int cropSeasonId, CancellationToken cancellationToken)
    {
        if (!await cropSeasons.ExistsAsync(cropSeasonId, cancellationToken))
            return NotFound(new { message = "Crop season not found.", cropSeasonId });

        var items = await farmers.ListForCropSeasonAsync(cropSeasonId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{farmerId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        Guid farmerId,
        [FromQuery] int cropSeasonId,
        CancellationToken cancellationToken)
    {
        if (!await cropSeasons.ExistsAsync(cropSeasonId, cancellationToken))
            return NotFound(new { message = "Crop season not found.", cropSeasonId });

        var detail = await farmers.GetDetailAsync(farmerId, cropSeasonId, cancellationToken);
        if (detail is null)
            return NotFound(new { message = "Farmer not found or has no data for this crop season context.", farmerId });

        return Ok(detail);
    }

    [HttpGet("by-code/{farmerCode}")]
    public async Task<IActionResult> GetByCodeAsync(
        string farmerCode,
        [FromQuery] int cropSeasonId,
        CancellationToken cancellationToken)
    {
        if (!await cropSeasons.ExistsAsync(cropSeasonId, cancellationToken))
            return NotFound(new { message = "Crop season not found.", cropSeasonId });

        var detail = await farmers.GetDetailByCodeAsync(farmerCode, cropSeasonId, cancellationToken);
        if (detail is null)
            return NotFound(new { message = "Farmer not found.", farmerCode });

        return Ok(detail);
    }
}
