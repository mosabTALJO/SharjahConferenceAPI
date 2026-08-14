namespace Data.Entities;

using System;
using System.Collections.Generic;
using Data.Enums;

public class MeetingTable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required Guid ZoneId { get; init; }
    public Zone? Zone { get; init; }
    
    public required string TableNumber { get; set; }
    
    public required double CoordX { get; set; }
    public required double CoordY { get; set; }
    
    public required int Capacity { get; set; }
    
    public TableStatus OperationalStatus { get; set; } = TableStatus.Available;

    public ICollection<Meeting> Meetings { get; init; } = [];
}
