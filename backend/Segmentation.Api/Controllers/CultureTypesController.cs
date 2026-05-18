using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Segmentation.Infrastructure.Persistence;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CultureTypesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var items = await db.CultureTypes
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new { c.Code, c.Name })
            .ToListAsync(cancellationToken);
        return Ok(items);
    }
}
