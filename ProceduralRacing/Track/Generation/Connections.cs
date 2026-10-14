using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

// There are two types of connections: LocalConnections and WorldConnections.
// LocalConnections are local to a track piece and define where the track enters and exits the piece.
// WorldConnections are absolute positions in the world grid and define where the next piece must connect.

public class LocalConnection
{
    // --- Properties ---
    public Point Position { get; }   // Position relative to the piece's top-left corner
    public Point Direction { get; }  // Direction the track is heading

    // --- Constructor ---
    public LocalConnection(Point position, Point direction)
    {
        Position = position;
        Direction = direction;
    }

    // --- Utility Helpers ---
    public bool IsOpposite(Point other) =>
        Direction == new Point(-other.X, -other.Y);

    public Point GetNextCell(Point worldPos) =>
        worldPos + Position + Direction;

    public bool LeadsToEmptySpace(Point pieceGridPos, Grid grid) =>
        !grid.IsOccupied(GetNextCell(pieceGridPos));

    public override int GetHashCode() =>
        HashCode.Combine(Position, Direction);
}


public class WorldConnection
{
    // --- Properties ---
    public Point GridPosition { get; }  // Absolute grid cell where next piece must connect
    public Point Direction { get; }     // Direction the track is heading

    // -- Constructor ---
    public WorldConnection(Point gridPosition, Point direction)
    {
        GridPosition = gridPosition;
        Direction = direction;
    }
}
