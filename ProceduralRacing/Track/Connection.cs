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

    public bool IsOpposite(Connection other)
    {
        return Direction == new Point(-other.Direction.X, -other.Direction.Y);
    }
    public bool LeadsToEmptySpace(Point pieceGridPos, Grid grid)
    {
        Point nextCell = pieceGridPos + Position + Direction;

        if (nextCell.X < grid.MinX || nextCell.X > grid.MaxX ||
            nextCell.Y < grid.MinY || nextCell.Y > grid.MaxY)
            return false;

        return !grid.IsOccupied(nextCell);
    }
}