namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class VenueMappers
{
    public static VenueDto ToDto(this Venue entity)
    {
        return new VenueDto(entity.Id, entity.NameEn, entity.NameAr);
    }

    public static Venue ToEntityFromCreate(this CreateVenueDto dto)
    {
        return new Venue
        {
            NameEn = dto.NameEn,
            NameAr = dto.NameAr
        };
    }
}
