namespace SharjahConferenceAPI.DTOs;

using System;
using Data.Enums;

public record MeetingTableDto(
    Guid Id,
    Guid ZoneId,
    string TableNumber,
    double CoordX,
    double CoordY,
    int Capacity,
    TableStatus OperationalStatus,
    Guid? ReservedByPublisherId = null,
    string? PublisherNameEn = null
);

public record CreateMeetingTableDto(Guid ZoneId, string TableNumber, double CoordX, double CoordY, int Capacity);

public record UpdateMeetingTableDto(string TableNumber, double CoordX, double CoordY, int Capacity, TableStatus OperationalStatus);

public record ReserveTableDto(Guid? PublisherId = null);
