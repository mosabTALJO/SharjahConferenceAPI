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
public class PublishersController(
    IPublisherService publisherService,
    IMeetingTableService meetingTableService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var publishers = await publisherService.GetAllAsync(cancellationToken);
        return Ok(publishers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var publisher = await publisherService.GetByIdAsync(id, cancellationToken);
        if (publisher == null) return NotFound();
        return Ok(publisher);
    }

    [HttpGet("{id:guid}/tables")]
    public async Task<IActionResult> GetPublisherTables(Guid id, CancellationToken cancellationToken)
    {
        var tables = await meetingTableService.GetByPublisherIdAsync(id, cancellationToken);
        return Ok(tables);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePublisherDto dto, CancellationToken cancellationToken)
    {
        var created = await publisherService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublisherDto dto, CancellationToken cancellationToken)
    {
        var updated = await publisherService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await publisherService.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
