namespace SharjahConferenceAPI.Controllers;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeetingTablesController(IMeetingTableService tableService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? zoneId,
        [FromQuery] Guid? publisherId,
        CancellationToken cancellationToken)
    {
        if (publisherId.HasValue)
        {
            var tablesByPublisher = await tableService.GetByPublisherIdAsync(publisherId.Value, cancellationToken);
            return Ok(tablesByPublisher);
        }

        if (zoneId.HasValue)
        {
            var tablesByZone = await tableService.GetByZoneIdAsync(zoneId.Value, cancellationToken);
            return Ok(tablesByZone);
        }

        var tables = await tableService.GetAllAsync(cancellationToken);
        return Ok(tables);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var table = await tableService.GetByIdAsync(id, cancellationToken);
        if (table == null) return NotFound();
        return Ok(table);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingTableDto dto, CancellationToken cancellationToken)
    {
        var created = await tableService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeetingTableDto dto, CancellationToken cancellationToken)
    {
        var updated = await tableService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await tableService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("{id:guid}/reserve")]
    public async Task<IActionResult> Reserve(
        Guid id,
        [FromBody] ReserveTableDto? dto,
        CancellationToken cancellationToken)
    {
        Guid publisherId;
        var isAdmin = User.IsInRole("Admin");

        if (isAdmin && dto?.PublisherId.HasValue == true)
        {
            publisherId = dto.PublisherId.Value;
        }
        else
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            if (publisherIdClaim == null || !Guid.TryParse(publisherIdClaim, out publisherId))
            {
                return Forbid("A valid publisher identity is required to reserve a table.");
            }
        }

        try
        {
            var result = await tableService.ReserveTableAsync(id, publisherId, cancellationToken);
            if (result == null) return NotFound(new { message = "Table not found." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,Publisher")]
    [HttpPost("{id:guid}/unreserve")]
    public async Task<IActionResult> Unreserve(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        Guid publisherId = Guid.Empty;

        if (!isAdmin)
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            if (publisherIdClaim == null || !Guid.TryParse(publisherIdClaim, out publisherId))
            {
                return Forbid("A valid publisher identity is required.");
            }
        }

        try
        {
            var success = await tableService.UnreserveTableAsync(id, publisherId, isAdmin, cancellationToken);
            if (!success) return NotFound(new { message = "Table not found." });
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
