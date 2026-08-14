namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class ZoneMappers
{
    public static ZoneDto ToDto(this Zone entity)
    {
        return new ZoneDto(entity.Id, entity.VenueId, entity.Code, entity.NameEn, entity.NameAr, entity.SvgBounds);
    }

    public static Zone ToEntityFromCreate(this CreateZoneDto dto)
    {
        return new Zone
        {
            VenueId = dto.VenueId,
            Code = dto.Code,
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            SvgBounds = dto.SvgBounds
        };
    }
}
