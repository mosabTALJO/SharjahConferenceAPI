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
public class ParticipantsController(IParticipantService participantService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var participants = await participantService.GetAllAsync(cancellationToken);
        return Ok(participants);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;
            if (participantIdClaim == null || !Guid.TryParse(participantIdClaim, out var callerParticipantId) || callerParticipantId != id)
            {
                return Forbid();
            }
        }

        var participant = await participantService.GetByIdAsync(id, cancellationToken);
        if (participant == null) return NotFound();
        return Ok(participant);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateParticipantDto dto, CancellationToken cancellationToken)
    {
        var created = await participantService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParticipantDto dto, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;
            if (participantIdClaim == null || !Guid.TryParse(participantIdClaim, out var callerParticipantId) || callerParticipantId != id)
            {
                return Forbid();
            }
        }

        var updated = await participantService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await participantService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
