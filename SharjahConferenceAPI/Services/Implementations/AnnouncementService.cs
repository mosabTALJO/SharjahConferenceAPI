namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class AnnouncementService : IAnnouncementService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnnouncementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AnnouncementDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Announcements.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<AnnouncementDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Announcements.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Announcements.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<AnnouncementDto?> UpdateAsync(Guid id, UpdateAnnouncementDto updateDto)
    {
        var entity = await _unitOfWork.Announcements.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Title = updateDto.Title;
        entity.Body = updateDto.Body;
        entity.Priority = updateDto.Priority;
        entity.ActiveStartTime = updateDto.ActiveStartTime;
        entity.ActiveEndTime = updateDto.ActiveEndTime;

        _unitOfWork.Announcements.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Announcements.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Announcements.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
