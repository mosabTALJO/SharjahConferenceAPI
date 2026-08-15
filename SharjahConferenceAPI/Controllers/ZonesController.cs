namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ZonesController(IZoneService zoneService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? venueId, CancellationToken cancellationToken)
    {
        if (venueId.HasValue)
        {
            var zonesByVenue = await zoneService.GetByVenueIdAsync(venueId.Value);
            return Ok(zonesByVenue);
        }
        var zones = await zoneService.GetAllAsync();
        return Ok(zones);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var zone = await zoneService.GetByIdAsync(id);
        if (zone == null) return NotFound();
        return Ok(zone);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateZoneDto dto, CancellationToken cancellationToken)
    {
        var created = await zoneService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateZoneDto dto, CancellationToken cancellationToken)
    {
        var updated = await zoneService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await zoneService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
