namespace SharjahConferenceAPI.DTOs;

using System;
using Data.Enums;

public record MeetingDto(Guid Id, Guid MeetingTableId, Guid PublisherId, Guid ParticipantId, DateTime StartTime, DateTime EndTime, MeetingStatus Status, string? Notes);

public record CreateMeetingDto(Guid MeetingTableId, Guid PublisherId, Guid ParticipantId, DateTime StartTime, DateTime EndTime, string? Notes);

public record UpdateMeetingDto(DateTime StartTime, DateTime EndTime, MeetingStatus Status, string? Notes);
