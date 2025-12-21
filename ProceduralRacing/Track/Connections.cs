using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using System.Collections.Generic;

public class Connection
{
    public Point Position { get; set; }
    public Point Direction { get; set; }

    public Connection(Point position, Point direction)
    {
        Position = position;
        Direction = direction;
    }

    // Check if this connection's direction is opposite to another connection's direction
    // Worth noting this can check both track connections and world connections as it just compares vectors
    public bool IsOpposite(Point other)
    {
        return Direction == new Point(-other.X, -other.Y);
    }
    public bool LeadsToEmptySpace(Point pieceGridPos, Grid grid)
    {
        Point worldPos = pieceGridPos + this.Position + this.Direction;
        return !grid.IsOccupied(worldPos);
    }
}

public class WorldConnection
{
    public Point GridPosition;   // Absolute grid cell where next piece must connect
    public Point Direction;      // Direction the track is heading

    public WorldConnection(Point gridPosition, Point direction)
    {
        GridPosition = gridPosition;
        Direction = direction;
    }
}
