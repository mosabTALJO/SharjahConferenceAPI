namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IVenueService
{
    Task<IEnumerable<VenueDto>> GetAllAsync();
    Task<VenueDto?> GetByIdAsync(Guid id);
    Task<VenueDto> CreateAsync(CreateVenueDto createDto);
    Task<VenueDto?> UpdateAsync(Guid id, UpdateVenueDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
