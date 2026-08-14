namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IMeetingService
{
    Task<IEnumerable<MeetingDto>> GetAllAsync();
    Task<MeetingDto?> GetByIdAsync(Guid id);
    Task<MeetingDto> CreateAsync(CreateMeetingDto createDto);
    Task<MeetingDto?> UpdateAsync(Guid id, UpdateMeetingDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
