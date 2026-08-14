namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class FacilityMappers
{
    public static FacilityDto ToDto(this Facility entity)
    {
        return new FacilityDto(entity.Id, entity.Category, entity.Name, entity.CoordX, entity.CoordY);
    }

    public static Facility ToEntityFromCreate(this CreateFacilityDto dto)
    {
        return new Facility
        {
            Category = dto.Category,
            Name = dto.Name,
            CoordX = dto.CoordX,
            CoordY = dto.CoordY
        };
    }
}
