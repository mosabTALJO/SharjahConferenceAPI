namespace SharjahConferenceAPI.DTOs;

using System;

public record VenueDto(Guid Id, string NameEn, string NameAr);

public record CreateVenueDto(string NameEn, string NameAr);

public record UpdateVenueDto(string NameEn, string NameAr);
