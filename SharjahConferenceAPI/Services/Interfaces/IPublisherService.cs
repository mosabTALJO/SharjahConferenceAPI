namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IPublisherService
{
    Task<IEnumerable<PublisherDto>> GetAllAsync();
    Task<PublisherDto?> GetByIdAsync(Guid id);
    Task<PublisherDto> CreateAsync(CreatePublisherDto createDto);
    Task<PublisherDto?> UpdateAsync(Guid id, UpdatePublisherDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
