namespace SharjahConferenceAPI.Mappers;

using SharjahConferenceAPI.DTOs;
using Data.Entities;

public static class PublisherMappers
{
    public static PublisherDto ToDto(this Publisher entity)
    {
        return new PublisherDto(entity.Id, entity.NameEn, entity.NameAr, entity.CountryCode);
    }

    public static Publisher ToEntityFromCreate(this CreatePublisherDto dto)
    {
        return new Publisher
        {
            NameEn = dto.NameEn,
            NameAr = dto.NameAr,
            CountryCode = dto.CountryCode
        };
    }
}
