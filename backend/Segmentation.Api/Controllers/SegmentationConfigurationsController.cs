using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Segmentation.Application.Abstractions;
using Segmentation.Application.Dtos;
using Segmentation.Application.Exceptions;

namespace Segmentation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SegmentationConfigurationsController(ISegmentationConfigurationService configurations) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var items = await configurations.ListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}", Name = "GetSegmentationConfiguration")]
    public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await configurations.GetAsync(id, cancellationToken);
        if (detail is null)
            return NotFound(new { message = "Segmentation configuration not found.", id });

        return Ok(detail);
    }

    [HttpPatch("{id:guid}/name")]
    public async Task<IActionResult> PatchNameAsync(
        Guid id,
        [FromBody] PatchSegmentationConfigurationNameDto body,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await configurations.PatchNameAsync(id, body.Name, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message, id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, param = ex.ParamName });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await configurations.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetSegmentationConfiguration", new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, param = ex.ParamName });
        }
        catch (SegmentationConfigurationValidationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                ex.SumOfKpiMaxScores,
                ex.MaximumScore
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] SaveSegmentationConfigurationDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await configurations.UpdateAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message, id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message, param = ex.ParamName });
        }
        catch (SegmentationConfigurationValidationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                ex.SumOfKpiMaxScores,
                ex.MaximumScore
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Conflict(new
            {
                message =
                    "Save failed because the database reported no matching row for an update/delete. See concurrency.entries for which entities EF tried to persist.",
                detail = ex.InnerException?.Message ?? ex.Message,
                concurrency = BuildConcurrencyConflictDetail(ex),
            });
        }
    }

    /// <summary>
    /// Surfaces EF entity types, states, and primary keys so API clients can pinpoint failing rows without attaching a debugger.
    /// </summary>
    private static object BuildConcurrencyConflictDetail(DbUpdateConcurrencyException ex)
    {
        var entries = new List<object>();
        foreach (EntityEntry entry in ex.Entries)
        {
            var keyValues = entry.Metadata.FindPrimaryKey() is { } key
                ? key.Properties.ToDictionary(p => p.Name, p => entry.Property(p.Name).CurrentValue)
                : new Dictionary<string, object?>();

            entries.Add(new
            {
                entityType = entry.Metadata.ClrType.FullName,
                state = entry.State.ToString(),
                keyValues,
            });
        }

        return new
        {
            entryCount = entries.Count,
            entries,
            ex.Message,
        };
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> DuplicateAsync(
        Guid id,
        [FromBody] DuplicateSegmentationConfigurationRequest? body,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await configurations.DuplicateAsync(id, body?.Name, cancellationToken);
            return CreatedAtRoute("GetSegmentationConfiguration", new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message, id });
        }
        catch (SegmentationConfigurationValidationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                ex.SumOfKpiMaxScores,
                ex.MaximumScore
            });
        }
    }
}
