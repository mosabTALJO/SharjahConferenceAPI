namespace SharjahConferenceAPI.DTOs;

using System;

public record ZoneDto(Guid Id, Guid VenueId, string Code, string NameEn, string NameAr, string SvgBounds);

public record CreateZoneDto(Guid VenueId, string Code, string NameEn, string NameAr, string SvgBounds);

public record UpdateZoneDto(string Code, string NameEn, string NameAr, string SvgBounds);
