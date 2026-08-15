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

public class PublisherService(IUnitOfWork unitOfWork) : IPublisherService
{
    public async Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.Publishers.GetAllAsync(cancellationToken);
        return entities.Select(e => e.ToDto());
    }

    public async Task<PublisherDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Publishers.GetByIdAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<PublisherDto> CreateAsync(CreatePublisherDto createDto, CancellationToken cancellationToken = default)
    {
        var entity = createDto.ToEntityFromCreate();
        await unitOfWork.Publishers.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return entity.ToDto();
    }

    public async Task<PublisherDto?> UpdateAsync(Guid id, UpdatePublisherDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Publishers.GetByIdAsync(id, cancellationToken);
        if (entity == null) return null;

        entity.NameEn = updateDto.NameEn;
        entity.NameAr = updateDto.NameAr;
        entity.CountryCode = updateDto.CountryCode;

        unitOfWork.Publishers.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);

        return entity.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Publishers.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        unitOfWork.Publishers.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return true;
    }
}
