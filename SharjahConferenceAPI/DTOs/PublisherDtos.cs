namespace SharjahConferenceAPI.DTOs;

using System;

public record PublisherDto(Guid Id, string NameEn, string NameAr, string CountryCode);

public record CreatePublisherDto(string NameEn, string NameAr, string CountryCode);

public record UpdatePublisherDto(string NameEn, string NameAr, string CountryCode);
