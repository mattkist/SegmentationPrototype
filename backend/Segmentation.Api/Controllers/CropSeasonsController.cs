using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CropSeasonsController(ICropSeasonReadService cropSeasons) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var items = await cropSeasons.ListAsync(cancellationToken);
        return Ok(items);
    }
}
