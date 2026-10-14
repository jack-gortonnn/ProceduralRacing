using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

// The Grid is used to manage occupied cells in the track generation process.
// It provides methods to occupy/unoccupy cells, check occupancy, and convert grid positions to world positions.

public class Grid
{
    // --- Properties ---

    private readonly Dictionary<Point, bool> occupied = new();

    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public int MinY { get; private set; }
    public int MaxY { get; private set; }

    public float TileSize { get; private set; }

    // --- Constructor ---

    public Grid(int minX, int maxX, int minY, int maxY, float tileSize = 32f)
    { // Sets the grid's bounds and cell size

        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
        TileSize = tileSize;
    }

    // --- Cell Operations ---

    public void Clear() => occupied.Clear();

    public bool IsOccupied(Point cell) => occupied.ContainsKey(cell) && occupied[cell];

    public void OccupyCell(Point cell) => occupied[cell] = true;

    public void UnoccupyCell(Point cell) => occupied[cell] = false;

    public Vector2 ToWorldPosition(Point cell) => cell.ToVector2() * TileSize;

    // --- Rectangle & Bounds Helpers ---

    public void OccupyRectangle(Point topLeft, Point size)
    { // Marks every cell in the rectangle as occupied

        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                OccupyCell(new Point(topLeft.X + x, topLeft.Y + y));
    }

    public void UnoccupyRectangle(Point topLeft, Point size)
    { // Marks every cell in the rectangle as unoccupied

        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
                UnoccupyCell(new Point(topLeft.X + x, topLeft.Y + y));
    }

    // --- Piece Helpers ---

    public void Occupy(PlacedPiece piece) => OccupyRectangle(piece.GridPosition, piece.TransformedSize);

    public void Unoccupy(PlacedPiece piece) => UnoccupyRectangle(piece.GridPosition, piece.TransformedSize);

    public bool IsRectangleOccupied(Point topLeft, Point size)
    { // Checks whether any cell in the rectangle is already occupied

        for (int x = 0; x < size.X; x++)
            for (int y = 0; y < size.Y; y++)
            {
                if (IsOccupied(new Point(topLeft.X + x, topLeft.Y + y))) return true;
            }
        return false;
    }

    public bool IsInBounds(Point cell)
    { // Checks whether a cell falls within the grid's bounds

        return cell.X >= MinX && cell.X <= MaxX &&
               cell.Y >= MinY && cell.Y <= MaxY;
    }

    public bool IsRectangleInBounds(Point topLeft, Point size)
    { // Checks whether both corners of the rectangle fall within the grid's bounds

        return IsInBounds(topLeft) &&
               IsInBounds(new Point(topLeft.X + size.X - 1, topLeft.Y + size.Y - 1));
    }

    public bool DoesRectangleContain(Point rectPosition, Point rectSize, Point point)
    { // Checks whether a point falls within the given rectangle

        return point.X >= rectPosition.X &&
               point.Y >= rectPosition.Y &&
               point.X < rectPosition.X + rectSize.X &&
               point.Y < rectPosition.Y + rectSize.Y;
    }

    // --- Pathfinding ---

    public bool HasPath(Point from, Point to)
    { // Checks whether 'to' is reachable from 'from' via unoccupied, in-bounds cells

        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();
        queue.Enqueue(from);
        visited.Add(from);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == to) return true;

            foreach (var dir in new[] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) })
            {
                var next = current + dir;
                if (!visited.Contains(next) && IsInBounds(next) && !IsOccupied(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }
        return false;
    }

    // --- Rendering ---

    public void DrawGridBackground(SpriteBatch spriteBatch, Texture2D texture)
    { // Tiles the background texture across the grid's world-space area

        int texWidth = texture.Width;
        int texHeight = texture.Height;

        float worldWidth = (MaxX - MinX + 1) * TileSize;
        float worldHeight = (MaxY - MinY + 1) * TileSize;

        for (float x = MinX * TileSize; x < MinX * TileSize + worldWidth; x += texWidth)
        {
            for (float y = MinY * TileSize; y < MinY * TileSize + worldHeight; y += texHeight)
            {
                spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
            }
        }
    }
}
