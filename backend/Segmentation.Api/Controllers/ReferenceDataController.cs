using Microsoft.AspNetCore.Mvc;
using Segmentation.Application.Abstractions;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReferenceDataController(IReferenceDataReadService referenceData) : ControllerBase
{
    [HttpGet("technologies")]
    public async Task<IActionResult> TechnologiesAsync(CancellationToken cancellationToken)
    {
        var items = await referenceData.ListTechnologiesAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("irregularity-types")]
    public async Task<IActionResult> IrregularityTypesAsync(CancellationToken cancellationToken)
    {
        var items = await referenceData.ListIrregularityTypesAsync(cancellationToken);
        return Ok(items);
    }
}
