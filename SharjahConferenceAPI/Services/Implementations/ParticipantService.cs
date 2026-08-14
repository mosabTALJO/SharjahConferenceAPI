namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class ParticipantService : IParticipantService
{
    private readonly IUnitOfWork _unitOfWork;

    public ParticipantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ParticipantDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Participants.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<ParticipantDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Participants.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<ParticipantDto> CreateAsync(CreateParticipantDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Participants.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<ParticipantDto?> UpdateAsync(Guid id, UpdateParticipantDto updateDto)
    {
        var entity = await _unitOfWork.Participants.GetByIdAsync(id);
        if (entity == null) return null;

        entity.FirstName = updateDto.FirstName;
        entity.LastName = updateDto.LastName;
        entity.Email = updateDto.Email;
        entity.Phone = updateDto.Phone;
        entity.Title = updateDto.Title;

        _unitOfWork.Participants.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Participants.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Participants.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
