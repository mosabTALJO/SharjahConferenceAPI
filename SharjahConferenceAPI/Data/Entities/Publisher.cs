namespace Data.Entities;

using System;
using System.Collections.Generic;

public class Publisher
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string NameEn { get; set; }
    public required string NameAr { get; set; }
    public required string CountryCode { get; set; }

    public ICollection<Meeting> Meetings { get; init; } = [];
}
