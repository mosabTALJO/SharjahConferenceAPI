namespace Data.Entities;

using System;
using Data.Enums;

public class Meeting
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required Guid MeetingTableId { get; init; }
    public MeetingTable? MeetingTable { get; init; }
    
    public required Guid PublisherId { get; init; }
    public Publisher? Publisher { get; init; }
    
    public required Guid ParticipantId { get; init; }
    public Participant? Participant { get; init; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public string? Notes { get; set; }
}
