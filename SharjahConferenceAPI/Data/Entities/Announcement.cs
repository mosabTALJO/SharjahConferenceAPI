namespace Data.Entities;

using System;
using Data.Enums;

public class Announcement
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Title { get; set; }
    public required string Body { get; set; }
    
    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
    
    public DateTime ActiveStartTime { get; set; }
    public DateTime ActiveEndTime { get; set; }
}
