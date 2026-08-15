namespace Data.Entities;

using System;
using Data.Enums;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Email { get; set; }
    
    public required string PasswordHash { get; set; }
    
    public UserRole Role { get; set; }
    
    public Guid? PublisherId { get; set; }
    public Publisher? Publisher { get; set; }
    
    public Guid? ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
