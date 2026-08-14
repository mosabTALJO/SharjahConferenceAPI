namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class FacilityService : IFacilityService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacilityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FacilityDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Facilities.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<FacilityDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Facilities.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<FacilityDto> CreateAsync(CreateFacilityDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Facilities.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<FacilityDto?> UpdateAsync(Guid id, UpdateFacilityDto updateDto)
    {
        var entity = await _unitOfWork.Facilities.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Category = updateDto.Category;
        entity.Name = updateDto.Name;
        entity.CoordX = updateDto.CoordX;
        entity.CoordY = updateDto.CoordY;

        _unitOfWork.Facilities.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Facilities.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Facilities.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
