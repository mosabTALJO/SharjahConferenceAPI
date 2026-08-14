namespace Data.Entities;

using System;

public class Facility
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Category { get; init; }
    public required string Name { get; init; }
    
    public required double CoordX { get; init; }
    public required double CoordY { get; init; }
}
