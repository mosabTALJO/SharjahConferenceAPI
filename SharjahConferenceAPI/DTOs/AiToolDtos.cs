namespace SharjahConferenceAPI.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Result entry for fuzzy conference directory search (participants and publishers).
/// Does not expose sensitive contact details (emails, phone numbers).
/// </summary>
public record DirectorySearchResultDto(
    string EntryType,
    string Name,
    string? Title,
    string? Organization,
    string? Country,
    string? AssignedZone,
    string? AssignedTable,
    string? AdditionalInfo
);

/// <summary>
/// Status and schedule details for a specific meeting table.
/// </summary>
public record TableStatusResultDto(
    string TableNumber,
    string ZoneCode,
    string ZoneName,
    double CoordX,
    double CoordY,
    int Capacity,
    string Status,
    string? ReservedByPublisher,
    DateTime? CurrentMeetingStartTime,
    DateTime? CurrentMeetingEndTime,
    DateTime? NextScheduledMeetingStartTime,
    DateTime? NextScheduledMeetingEndTime,
    string? NextMeetingPublisherName,
    string? NextMeetingParticipantName
);

/// <summary>
/// Timetable and scheduled meetings for a conference participant.
/// </summary>
public record ParticipantScheduleResultDto(
    string ParticipantName,
    string Title,
    IReadOnlyList<ParticipantMeetingScheduleDto> ScheduledMeetings
);

/// <summary>
/// Individual scheduled meeting item for a participant.
/// </summary>
public record ParticipantMeetingScheduleDto(
    Guid MeetingId,
    DateTime StartTime,
    DateTime EndTime,
    string Status,
    string TableNumber,
    string? ZoneCode,
    string? ZoneName,
    string PublisherName,
    string? Notes
);

/// <summary>
/// Location and coordinate details for a facility or point of interest.
/// </summary>
public record FacilityLocationDto(
    Guid Id,
    string Name,
    string Category,
    double CoordX,
    double CoordY,
    string? NearestZoneCode,
    string? NearestZoneName
);

/// <summary>
/// Active public announcement broadcast to conference attendees.
/// </summary>
public record ActiveAnnouncementDto(
    Guid Id,
    string Title,
    string Body,
    string Priority,
    DateTime ActiveStartTime,
    DateTime ActiveEndTime
);

/// <summary>
/// Result containing all meeting tables reserved by a specific publisher.
/// </summary>
public record PublisherReservedTablesResultDto(
    Guid PublisherId,
    string PublisherNameEn,
    string PublisherNameAr,
    string CountryCode,
    IReadOnlyList<ReservedTableDetailDto> ReservedTables
);

/// <summary>
/// Detailed information about an individual meeting table reserved by a publisher.
/// </summary>
public record ReservedTableDetailDto(
    Guid TableId,
    string TableNumber,
    string ZoneCode,
    string ZoneName,
    double CoordX,
    double CoordY,
    int Capacity,
    string Status
);

