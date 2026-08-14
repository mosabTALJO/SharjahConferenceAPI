namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly IZoneService _zoneService;

    public ZonesController(IZoneService zoneService)
    {
        _zoneService = zoneService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? venueId)
    {
        if (venueId.HasValue)
        {
            var zonesByVenue = await _zoneService.GetByVenueIdAsync(venueId.Value);
            return Ok(zonesByVenue);
        }
        var zones = await _zoneService.GetAllAsync();
        return Ok(zones);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var zone = await _zoneService.GetByIdAsync(id);
        if (zone == null) return NotFound();
        return Ok(zone);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateZoneDto dto)
    {
        var created = await _zoneService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateZoneDto dto)
    {
        var updated = await _zoneService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _zoneService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
