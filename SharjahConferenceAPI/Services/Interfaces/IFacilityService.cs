namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IFacilityService
{
    Task<IEnumerable<FacilityDto>> GetAllAsync();
    Task<FacilityDto?> GetByIdAsync(Guid id);
    Task<FacilityDto> CreateAsync(CreateFacilityDto createDto);
    Task<FacilityDto?> UpdateAsync(Guid id, UpdateFacilityDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
