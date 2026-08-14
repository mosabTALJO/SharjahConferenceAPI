namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class ParticipantMappers
{
    public static ParticipantDto ToDto(this Participant entity)
    {
        return new ParticipantDto(entity.Id, entity.FirstName, entity.LastName, entity.Email, entity.Phone, entity.Title);
    }

    public static Participant ToEntityFromCreate(this CreateParticipantDto dto)
    {
        return new Participant
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Title = dto.Title
        };
    }
}
