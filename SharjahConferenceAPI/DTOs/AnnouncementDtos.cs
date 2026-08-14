namespace SharjahConferenceAPI.DTOs;

using System;
using Data.Enums;

public record AnnouncementDto(Guid Id, string Title, string Body, AnnouncementPriority Priority, DateTime ActiveStartTime, DateTime ActiveEndTime);

public record CreateAnnouncementDto(string Title, string Body, AnnouncementPriority Priority, DateTime ActiveStartTime, DateTime ActiveEndTime);

public record UpdateAnnouncementDto(string Title, string Body, AnnouncementPriority Priority, DateTime ActiveStartTime, DateTime ActiveEndTime);
