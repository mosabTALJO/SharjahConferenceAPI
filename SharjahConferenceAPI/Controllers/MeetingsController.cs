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
public class MeetingsController(IMeetingService meetingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        Guid? publisherId = null;
        Guid? participantId = null;

        if (Guid.TryParse(User.FindFirst("PublisherId")?.Value, out var pubId))
        {
            publisherId = pubId;
        }

        if (Guid.TryParse(User.FindFirst("ParticipantId")?.Value, out var partId))
        {
            participantId = partId;
        }

        var meetings = await meetingService.GetForUserAsync(role, publisherId, participantId, cancellationToken);
        return Ok(meetings);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var meeting = await meetingService.GetByIdAsync(id, cancellationToken);
        if (meeting == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;

            var isOwner = (Guid.TryParse(publisherIdClaim, out var pubId) && pubId == meeting.PublisherId)
                       || (Guid.TryParse(participantIdClaim, out var partId) && partId == meeting.ParticipantId);

            if (!isOwner) return Forbid();
        }

        return Ok(meeting);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingDto dto, CancellationToken cancellationToken)
    {
        CreateMeetingDto meetingToCreate = dto;

        if (User.IsInRole("Participant"))
        {
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;
            if (participantIdClaim == null || !Guid.TryParse(participantIdClaim, out var partId))
            {
                return Forbid("A valid participant identity is required.");
            }

            if (dto.ParticipantId != Guid.Empty && dto.ParticipantId != partId)
            {
                return Forbid("Participants can only book meetings for themselves.");
            }

            meetingToCreate = dto with { ParticipantId = partId };
        }
        else if (User.IsInRole("Publisher"))
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            if (publisherIdClaim == null || !Guid.TryParse(publisherIdClaim, out var pubId))
            {
                return Forbid("A valid publisher identity is required.");
            }

            if (dto.PublisherId != Guid.Empty && dto.PublisherId != pubId)
            {
                return Forbid("Publishers can only create meetings for themselves.");
            }

            meetingToCreate = dto with { PublisherId = pubId };
        }

        try
        {
            var created = await meetingService.CreateAsync(meetingToCreate, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeetingDto dto, CancellationToken cancellationToken)
    {
        var existing = await meetingService.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;

            var isOwner = (Guid.TryParse(publisherIdClaim, out var pubId) && pubId == existing.PublisherId)
                       || (Guid.TryParse(participantIdClaim, out var partId) && partId == existing.ParticipantId);

            if (!isOwner) return Forbid();
        }

        try
        {
            var updated = await meetingService.UpdateAsync(id, dto, cancellationToken);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existing = await meetingService.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var publisherIdClaim = User.FindFirst("PublisherId")?.Value;
            var participantIdClaim = User.FindFirst("ParticipantId")?.Value;

            var isOwner = (Guid.TryParse(publisherIdClaim, out var pubId) && pubId == existing.PublisherId)
                       || (Guid.TryParse(participantIdClaim, out var partId) && partId == existing.ParticipantId);

            if (!isOwner) return Forbid();
        }

        var deleted = await meetingService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
