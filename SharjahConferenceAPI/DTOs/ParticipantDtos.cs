namespace SharjahConferenceAPI.DTOs;

using System;

public record ParticipantDto(Guid Id, string FirstName, string LastName, string Email, string Phone, string Title);

public record CreateParticipantDto(string FirstName, string LastName, string Email, string Phone, string Title);

public record UpdateParticipantDto(string FirstName, string LastName, string Email, string Phone, string Title);
