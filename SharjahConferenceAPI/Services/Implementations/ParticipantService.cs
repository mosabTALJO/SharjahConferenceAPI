namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class ParticipantService(IUnitOfWork unitOfWork) : IParticipantService
{
    public async Task<IEnumerable<ParticipantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.Participants.GetAllAsync(cancellationToken);
        return entities.Select(e => e.ToDto());
    }

    public async Task<ParticipantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Participants.GetByIdAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<ParticipantDto> CreateAsync(CreateParticipantDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = createDto.ToEntityFromCreate();
        await unitOfWork.Participants.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<ParticipantDto?> UpdateAsync(Guid id, UpdateParticipantDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Participants.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        entity.FirstName = updateDto.FirstName;
        entity.LastName = updateDto.LastName;
        entity.Email = updateDto.Email;
        entity.Phone = updateDto.Phone;
        entity.Title = updateDto.Title;

        unitOfWork.Participants.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Participants.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        unitOfWork.Participants.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }
}
