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
public class AnnouncementsController(IAnnouncementService announcementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var announcements = await announcementService.GetAllAsync();
        return Ok(announcements);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var announcement = await announcementService.GetByIdAsync(id);
        if (announcement == null) return NotFound();
        return Ok(announcement);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementDto dto, CancellationToken cancellationToken)
    {
        var created = await announcementService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementDto dto, CancellationToken cancellationToken)
    {
        var updated = await announcementService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await announcementService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
