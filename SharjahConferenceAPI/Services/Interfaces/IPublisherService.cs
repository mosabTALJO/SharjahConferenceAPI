namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IPublisherService
{
    Task<IEnumerable<PublisherDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PublisherDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PublisherDto> CreateAsync(CreatePublisherDto createDto, CancellationToken cancellationToken = default);
    Task<PublisherDto?> UpdateAsync(Guid id, UpdatePublisherDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
