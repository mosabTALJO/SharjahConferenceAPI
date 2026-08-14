namespace Data.Entities;

using System;
using System.Collections.Generic;

public class Venue
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string NameEn { get; set; }
    public required string NameAr { get; set; }

    public ICollection<Zone> Zones { get; init; } = [];
}
