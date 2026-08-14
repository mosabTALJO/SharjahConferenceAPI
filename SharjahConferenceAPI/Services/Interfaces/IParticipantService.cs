namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IParticipantService
{
    Task<IEnumerable<ParticipantDto>> GetAllAsync();
    Task<ParticipantDto?> GetByIdAsync(Guid id);
    Task<ParticipantDto> CreateAsync(CreateParticipantDto createDto);
    Task<ParticipantDto?> UpdateAsync(Guid id, UpdateParticipantDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
