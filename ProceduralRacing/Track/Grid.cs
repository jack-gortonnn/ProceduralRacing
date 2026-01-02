using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

// The Grid is used to manage occupied cells in the track generation process.
// It provides methods to occupy/unoccupy cells, check occupancy, and convert grid positions to world positions.

public class Grid
{
    private readonly Dictionary<Point, bool> occupied = new();

    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public int MinY { get; private set; }
    public int MaxY { get; private set; }

    public float TileSize { get; private set; }

    public Grid(int minX, int maxX, int minY, int maxY, float tileSize = 32f)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
        TileSize = tileSize;
    }

    public void Clear() => occupied.Clear();

    public bool IsOccupied(Point cell) => occupied.ContainsKey(cell) && occupied[cell];

    public void OccupyCell(Point cell) => occupied[cell] = true;

    public void UnoccupyCell(Point cell) => occupied[cell] = false;

    public Vector2 ToWorldPosition(Point cell) => cell.ToVector2() * TileSize;

    public void OccupyRectangle(Point topLeft, Point size)
    {
        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                OccupyCell(new Point(topLeft.X + x, topLeft.Y + y));
    }

    public void UnoccupyRectangle(Point topLeft, Point size)
    {
        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                UnoccupyCell(new Point(topLeft.X + x, topLeft.Y + y));
    }

    public bool IsRectangleOccupied(Point topLeft, Point size)
    {
        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
            {
                if (IsOccupied(new Point(topLeft.X + x, topLeft.Y + y))) return true;
            }
        return false;
    }

    public bool IsInBounds(Point cell)
    {
        return cell.X >= MinX && cell.X <= MaxX &&
               cell.Y >= MinY && cell.Y <= MaxY;
    }

    public bool IsRectangleInBounds(Point topLeft, Point size)
    {
        return IsInBounds(topLeft) &&
               IsInBounds(new Point(topLeft.X + size.X - 1, topLeft.Y + size.Y - 1));
    }

    public bool DoesRectangleContain(Point rectPosition, Point rectSize, Point point)
    {
        return point.X >= rectPosition.X &&
               point.Y >= rectPosition.Y &&
               point.X < rectPosition.X + rectSize.X &&
               point.Y < rectPosition.Y + rectSize.Y;
    }
}
