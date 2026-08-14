namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IMeetingTableService
{
    Task<IEnumerable<MeetingTableDto>> GetAllAsync();
    Task<IEnumerable<MeetingTableDto>> GetByZoneIdAsync(Guid zoneId);
    Task<MeetingTableDto?> GetByIdAsync(Guid id);
    Task<MeetingTableDto> CreateAsync(CreateMeetingTableDto createDto);
    Task<MeetingTableDto?> UpdateAsync(Guid id, UpdateMeetingTableDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
