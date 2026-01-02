using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

// Connection represents a point on a track piece where it can connect to other pieces.
// It includes the position of the connection relative to the piece's top-left corner
// and the direction the track is heading from that connection point.

public class Connection
{
    // --- Properties ---
    public Point Position { get; set; }   // Position relative to the piece's top-left corner
    public Point Direction { get; set; }  // Direction the track is heading

    // --- Constructor ---
    public Connection(Point position, Point direction)
    {
        Position = position;
        Direction = direction;
    }

    // --- Overrides ---
    public override bool Equals(object? obj) =>
        obj is Connection other &&
        Position.Equals(other.Position) &&
        Direction.Equals(other.Direction);

    public override int GetHashCode() =>
        HashCode.Combine(Position, Direction);

    public static bool operator ==(Connection left, Connection right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(Connection left, Connection right) =>
        !(left == right);

    // --- Utility Helpers ---
    public bool IsOpposite(Point other) =>
        Direction == new Point(-other.X, -other.Y);

    public Point GetNextCell(Point worldPos) =>
        worldPos + Position + Direction;

    public bool LeadsToEmptySpace(Point pieceGridPos, Grid grid) =>
        !grid.IsOccupied(GetNextCell(pieceGridPos));
}


// World Connections represents a specific connection point in the world grid.
// It includes the absolute grid position where the next piece must connect.

public class WorldConnection
{
    // --- Properties ---
    public Point GridPosition;   // Absolute grid cell where next piece must connect
    public Point Direction;      // Direction the track is heading

    // -- Constructor ---
    public WorldConnection(Point gridPosition, Point direction)
    {
        GridPosition = gridPosition;
        Direction = direction;
    }
}
