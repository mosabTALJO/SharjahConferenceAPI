namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IMeetingService
{
    Task<IEnumerable<MeetingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MeetingDto>> GetForUserAsync(string role, Guid? publisherId, Guid? participantId, CancellationToken cancellationToken = default);
    Task<MeetingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MeetingDto> CreateAsync(CreateMeetingDto createDto, CancellationToken cancellationToken = default);
    Task<MeetingDto?> UpdateAsync(Guid id, UpdateMeetingDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
