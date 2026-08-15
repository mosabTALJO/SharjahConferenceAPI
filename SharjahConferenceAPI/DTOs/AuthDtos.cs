namespace SharjahConferenceAPI.DTOs;

using System;
using System.ComponentModel.DataAnnotations;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record AuthResponseDto(
    string Token,
    DateTime Expiration,
    string Role,
    string Email,
    Guid UserId,
    Guid? PublisherId,
    Guid? ParticipantId
);

public record RegisterParticipantDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    [Required, MaxLength(50)] string Phone,
    [Required, MaxLength(100)] string Title
);

public record RegisterPublisherDto(
    [Required, MaxLength(255)] string NameEn,
    [Required, MaxLength(255)] string NameAr,
    [Required, StringLength(3, MinimumLength = 2)] string CountryCode,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password
);

public record RegisterAdminDto(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password
);

public record CurrentUserDto(
    Guid UserId,
    string Email,
    string Role,
    Guid? PublisherId,
    Guid? ParticipantId,
    string? DisplayName
);
