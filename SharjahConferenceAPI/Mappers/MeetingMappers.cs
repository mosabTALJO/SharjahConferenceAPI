namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;
using Data.Enums;

public static class MeetingMappers
{
    public static MeetingDto ToDto(this Meeting entity)
    {
        return new MeetingDto(entity.Id, entity.MeetingTableId, entity.PublisherId, entity.ParticipantId, entity.StartTime, entity.EndTime, entity.Status, entity.Notes);
    }

    public static Meeting ToEntityFromCreate(this CreateMeetingDto dto)
    {
        return new Meeting
        {
            MeetingTableId = dto.MeetingTableId,
            PublisherId = dto.PublisherId,
            ParticipantId = dto.ParticipantId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = MeetingStatus.Scheduled, // default
            Notes = dto.Notes
        };
    }
}
