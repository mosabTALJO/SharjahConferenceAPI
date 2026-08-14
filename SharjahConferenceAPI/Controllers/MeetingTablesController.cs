namespace SharjahConferenceAPI.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class MeetingTablesController : ControllerBase
{
    private readonly IMeetingTableService _tableService;

    public MeetingTablesController(IMeetingTableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? zoneId)
    {
        if (zoneId.HasValue)
        {
            var tablesByZone = await _tableService.GetByZoneIdAsync(zoneId.Value);
            return Ok(tablesByZone);
        }
        var tables = await _tableService.GetAllAsync();
        return Ok(tables);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var table = await _tableService.GetByIdAsync(id);
        if (table == null) return NotFound();
        return Ok(table);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMeetingTableDto dto)
    {
        var created = await _tableService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeetingTableDto dto)
    {
        var updated = await _tableService.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _tableService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
