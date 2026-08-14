namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharjahConferenceAPI.DTOs;

public interface IAnnouncementService
{
    Task<IEnumerable<AnnouncementDto>> GetAllAsync();
    Task<AnnouncementDto?> GetByIdAsync(Guid id);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto createDto);
    Task<AnnouncementDto?> UpdateAsync(Guid id, UpdateAnnouncementDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
