namespace SharjahConferenceAPI.Services.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.UnitOfWork;
using SharjahConferenceAPI.DTOs;
using SharjahConferenceAPI.Mappers;
using SharjahConferenceAPI.Services.Interfaces;

public class PublisherService : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork;

    public PublisherService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PublisherDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Publishers.GetAllAsync();
        return entities.Select(e => e.ToDto());
    }

    public async Task<PublisherDto?> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Publishers.GetByIdAsync(id);
        return entity?.ToDto();
    }

    public async Task<PublisherDto> CreateAsync(CreatePublisherDto createDto)
    {
        var entity = createDto.ToEntityFromCreate();
        await _unitOfWork.Publishers.AddAsync(entity);
        await _unitOfWork.CompleteAsync();
        return entity.ToDto();
    }

    public async Task<PublisherDto?> UpdateAsync(Guid id, UpdatePublisherDto updateDto)
    {
        var entity = await _unitOfWork.Publishers.GetByIdAsync(id);
        if (entity == null) return null;

        entity.NameEn = updateDto.NameEn;
        entity.NameAr = updateDto.NameAr;
        entity.CountryCode = updateDto.CountryCode;

        _unitOfWork.Publishers.Update(entity);
        await _unitOfWork.CompleteAsync();

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Publishers.GetByIdAsync(id);
        if (entity == null) return false;

        _unitOfWork.Publishers.Remove(entity);
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
