namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class MeetingTableService : IMeetingTableService
{
    private readonly IUnitOfWork _unitOfWork;

    public MeetingTableService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MeetingTableDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.MeetingTables.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<MeetingTableDto>> GetByZoneIdAsync(Guid zoneId)
    {
        var entities = await _unitOfWork.MeetingTables.FindAsync(t => t.ZoneId == zoneId);
        return entities.Select(e => e.ToDto());
    }

    public async Task<MeetingTableDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.MeetingTables.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<MeetingTableDto> CreateAsync(CreateMeetingTableDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.MeetingTables.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<MeetingTableDto?> UpdateAsync(Guid id, UpdateMeetingTableDto updateDto)
    {
        var entity = await _unitOfWork.MeetingTables.GetByIdAsync(id);
        if (entity == null) return null;

        entity.TableNumber = updateDto.TableNumber;
        entity.CoordX = updateDto.CoordX;
        entity.CoordY = updateDto.CoordY;
        entity.Capacity = updateDto.Capacity;
        entity.OperationalStatus = updateDto.OperationalStatus;

        _unitOfWork.MeetingTables.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.MeetingTables.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.MeetingTables.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
