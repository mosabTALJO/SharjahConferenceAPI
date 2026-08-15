namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class MeetingTableMappers
{
    public static MeetingTableDto ToDto(this MeetingTable entity)
    {
        return new MeetingTableDto(
            entity.Id,
            entity.ZoneId,
            entity.TableNumber,
            entity.CoordX,
            entity.CoordY,
            entity.Capacity,
            entity.OperationalStatus,
            entity.ReservedByPublisherId,
            entity.ReservedByPublisher?.NameEn
        );
    }

    public static MeetingTable ToEntityFromCreate(this CreateMeetingTableDto dto)
    {
        return new MeetingTable
        {
            ZoneId = dto.ZoneId,
            TableNumber = dto.TableNumber,
            CoordX = dto.CoordX,
            CoordY = dto.CoordY,
            Capacity = dto.Capacity
        };
    }
}
