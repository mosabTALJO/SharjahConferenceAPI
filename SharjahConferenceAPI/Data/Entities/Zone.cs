namespace Data.Entities;

using System;
using System.Collections.Generic;

public class Zone
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required Guid VenueId { get; init; }
    public Venue? Venue { get; init; }
    
    public required string Code { get; set; }
    public required string NameEn { get; set; }
    public required string NameAr { get; set; }
    
    public required string SvgBounds { get; set; }

    public ICollection<MeetingTable> Tables { get; init; } = [];
}
