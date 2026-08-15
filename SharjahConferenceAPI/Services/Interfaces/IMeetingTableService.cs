namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IMeetingTableService
{
    Task<IEnumerable<MeetingTableDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<MeetingTableDto>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MeetingTableDto>> GetByPublisherIdAsync(Guid publisherId, CancellationToken cancellationToken = default);
    Task<MeetingTableDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MeetingTableDto> CreateAsync(CreateMeetingTableDto createDto, CancellationToken cancellationToken = default);
    Task<MeetingTableDto?> UpdateAsync(Guid id, UpdateMeetingTableDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MeetingTableDto?> ReserveTableAsync(Guid tableId, Guid publisherId, CancellationToken cancellationToken = default);
    Task<bool> UnreserveTableAsync(Guid tableId, Guid publisherId, bool isAdmin, CancellationToken cancellationToken = default);
}
