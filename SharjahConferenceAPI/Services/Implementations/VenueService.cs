namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class VenueService : IVenueService
{
    private readonly IUnitOfWork _unitOfWork;

    public VenueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VenueDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Venues.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<VenueDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Venues.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<VenueDto> CreateAsync(CreateVenueDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Venues.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<VenueDto?> UpdateAsync(Guid id, UpdateVenueDto updateDto)
    {
        var entity = await _unitOfWork.Venues.GetByIdAsync(id);
        if (entity == null) return null;

        entity.NameEn = updateDto.NameEn;
        entity.NameAr = updateDto.NameAr;

        _unitOfWork.Venues.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Venues.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Venues.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
