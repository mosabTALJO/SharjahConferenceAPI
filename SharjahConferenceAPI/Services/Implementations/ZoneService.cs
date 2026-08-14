namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class ZoneService : IZoneService
{
    private readonly IUnitOfWork _unitOfWork;

    public ZoneService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ZoneDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Zones.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<IEnumerable<ZoneDto>> GetByVenueIdAsync(Guid venueId)
    {
        var entities = await _unitOfWork.Zones.FindAsync(z => z.VenueId == venueId);
        return entities.Select(e => e.ToDto());
    }

    public async Task<ZoneDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Zones.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<ZoneDto> CreateAsync(CreateZoneDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Zones.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<ZoneDto?> UpdateAsync(Guid id, UpdateZoneDto updateDto)
    {
        var entity = await _unitOfWork.Zones.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Code = updateDto.Code;
        entity.NameEn = updateDto.NameEn;
        entity.NameAr = updateDto.NameAr;
        entity.SvgBounds = updateDto.SvgBounds;

        _unitOfWork.Zones.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Zones.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Zones.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
