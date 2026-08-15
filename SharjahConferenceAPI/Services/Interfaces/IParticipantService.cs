namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IParticipantService
{
    Task<IEnumerable<ParticipantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ParticipantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParticipantDto> CreateAsync(CreateParticipantDto createDto, CancellationToken cancellationToken = default);
    Task<ParticipantDto?> UpdateAsync(Guid id, UpdateParticipantDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
