namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class MeetingService : IMeetingService
{
    private readonly IUnitOfWork _unitOfWork;

    public MeetingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MeetingDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Meetings.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<MeetingDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Meetings.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<MeetingDto> CreateAsync(CreateMeetingDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Meetings.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<MeetingDto?> UpdateAsync(Guid id, UpdateMeetingDto updateDto)
    {
        var entity = await _unitOfWork.Meetings.GetByIdAsync(id);
        if (entity == null) return null;

        entity.StartTime = updateDto.StartTime;
        entity.EndTime = updateDto.EndTime;
        entity.Status = updateDto.Status;
        entity.Notes = updateDto.Notes;

        _unitOfWork.Meetings.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Meetings.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Meetings.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
