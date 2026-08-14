namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var venues = await _venueService.GetAllAsync();
        return Ok(venues);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var venue = await _venueService.GetByIdAsync(id);
        if (venue == null) return NotFound();
        return Ok(venue);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVenueDto dto)
    {
        var created = await _venueService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVenueDto dto)
    {
        var updated = await _venueService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _venueService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
