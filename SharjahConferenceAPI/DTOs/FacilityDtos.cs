namespace SharjahConferenceAPI.DTOs;

using System;

public record FacilityDto(Guid Id, string Category, string Name, double CoordX, double CoordY);

public record CreateFacilityDto(string Category, string Name, double CoordX, double CoordY);

public record UpdateFacilityDto(string Category, string Name, double CoordX, double CoordY);
