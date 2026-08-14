namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly IParticipantService _participantService;

    public ParticipantsController(IParticipantService participantService)
    {
        _participantService = participantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var participants = await _participantService.GetAllAsync();
        return Ok(participants);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var participant = await _participantService.GetByIdAsync(id);
        if (participant == null) return NotFound();
        return Ok(participant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParticipantDto dto)
    {
        var created = await _participantService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParticipantDto dto)
    {
        var updated = await _participantService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _participantService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
