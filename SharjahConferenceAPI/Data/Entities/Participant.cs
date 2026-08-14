namespace Data.Entities;

using System;
using System.Collections.Generic;

public class Participant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Title { get; set; }

    public ICollection<Meeting> Meetings { get; init; } = [];
}
