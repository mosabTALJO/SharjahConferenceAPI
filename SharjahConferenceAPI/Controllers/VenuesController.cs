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
public class VenuesController(IVenueService venueService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var venues = await venueService.GetAllAsync();
        return Ok(venues);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var venue = await venueService.GetByIdAsync(id);
        if (venue == null) return NotFound();
        return Ok(venue);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVenueDto dto, CancellationToken cancellationToken)
    {
        var created = await venueService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVenueDto dto, CancellationToken cancellationToken)
    {
        var updated = await venueService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await venueService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
