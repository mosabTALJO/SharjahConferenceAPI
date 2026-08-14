namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class FacilitiesController : ControllerBase
{
    private readonly IFacilityService _facilityService;

    public FacilitiesController(IFacilityService facilityService)
    {
        _facilityService = facilityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var facilities = await _facilityService.GetAllAsync();
        return Ok(facilities);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var facility = await _facilityService.GetByIdAsync(id);
        if (facility == null) return NotFound();
        return Ok(facility);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFacilityDto dto)
    {
        var created = await _facilityService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFacilityDto dto)
    {
        var updated = await _facilityService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _facilityService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
