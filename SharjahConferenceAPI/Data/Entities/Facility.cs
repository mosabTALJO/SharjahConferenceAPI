namespace Data.Entities;

using System;

public class Facility
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Category { get; set; }
    public required string Name { get; set; }
    
    public required double CoordX { get; set; }
    public required double CoordY { get; set; }
}
