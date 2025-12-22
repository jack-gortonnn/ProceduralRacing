using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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
                if (IsOccupied(new Point(topLeft.X + x, topLeft.Y + y)))
                    return true;
            }

        return false;
    }

    public bool IsRectangleInBounds(Point topLeft, Point size)
    {
        return topLeft.X >= MinX && topLeft.Y >= MinY &&
               (topLeft.X + size.X - 1) <= MaxX &&
               (topLeft.Y + size.Y - 1) <= MaxY;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (spriteBatch == null || pixel == null) return;

        // Draw cells
        for (int x = MinX; x <= MaxX; x++)
        {
            for (int y = MinY; y <= MaxY; y++)
            {
                Point cell = new(x, y);
                bool occupiedCell = IsOccupied(cell);
                Color fillColor = occupiedCell ? Color.Red * 0.5f : Color.Green * 0.05f;

                spriteBatch.Draw(pixel, ToWorldPosition(cell), null, fillColor, 0f,
                    Vector2.Zero, new Vector2(TileSize, TileSize), SpriteEffects.None, 0f); 
            }
        }

        // Draw vertical grid lines
        Color lineColor = Color.White * 0.25f;
        for (int x = MinX; x <= MaxX + 1; x++)
        {
            Vector2 pos = ToWorldPosition(new Point(x, MinY));
            spriteBatch.Draw(pixel, pos, null, lineColor, 0f,
                Vector2.Zero, new Vector2(1, (MaxY - MinY + 1) * TileSize), SpriteEffects.None, 0f);
        }

        // Draw horizontal grid lines
        for (int y = MinY; y <= MaxY + 1; y++)
        {
            Vector2 pos = ToWorldPosition(new Point(MinX, y));
            spriteBatch.Draw(pixel, pos, null, lineColor, 0f,
                Vector2.Zero, new Vector2((MaxX - MinX + 1) * TileSize, 1), SpriteEffects.None, 0f);
        }
    }
}
