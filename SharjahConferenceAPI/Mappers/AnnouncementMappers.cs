namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class AnnouncementMappers
{
    public static AnnouncementDto ToDto(this Announcement entity)
    {
        return new AnnouncementDto(entity.Id, entity.Title, entity.Body, entity.Priority, entity.ActiveStartTime, entity.ActiveEndTime);
    }

    public static Announcement ToEntityFromCreate(this CreateAnnouncementDto dto)
    {
        return new Announcement
        {
            Title = dto.Title,
            Body = dto.Body,
            Priority = dto.Priority,
            ActiveStartTime = dto.ActiveStartTime,
            ActiveEndTime = dto.ActiveEndTime
        };
    }
}
