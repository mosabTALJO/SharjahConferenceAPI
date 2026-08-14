namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IZoneService
{
    Task<IEnumerable<ZoneDto>> GetAllAsync();
    Task<IEnumerable<ZoneDto>> GetByVenueIdAsync(Guid venueId);
    Task<ZoneDto?> GetByIdAsync(Guid id);
    Task<ZoneDto> CreateAsync(CreateZoneDto createDto);
    Task<ZoneDto?> UpdateAsync(Guid id, UpdateZoneDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
