using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public class PlacedPiece
{
    // Original properties
    public TrackPiece BasePiece { get; }
    public Point GridPosition { get; }
    public int Rotation { get; } // 0–3
    public bool IsFlipped { get; }

    // Transformed properties
    public Point TransformedSize { get; }
    public List<Connection> TransformedConnections { get; }

    // Constructor
    public PlacedPiece(TrackPiece basePiece, Point gridPosition, int rotation = 0, bool isFlipped = false)
    {
        BasePiece = basePiece ?? throw new ArgumentNullException(nameof(basePiece));
        GridPosition = gridPosition;
        Rotation = ((rotation % 4) + 4) % 4;
        IsFlipped = isFlipped;

        // Compute transformed size
        TransformedSize = (Rotation % 2 == 0) ? BasePiece.Size : new Point(BasePiece.Size.Y, BasePiece.Size.X);

        // Compute transformed connections
        TransformedConnections = new List<Connection>();
        int w = BasePiece.Size.X;
        int h = BasePiece.Size.Y;
        foreach (var con in BasePiece.Connections)
        {
            Point pos = con.Position;
            Point dir = con.Direction;
            if (IsFlipped)
            {
                pos = new Point(w - 1 - pos.X, pos.Y);
                dir = new Point(-dir.X, dir.Y);
            }
            Point finalPos = Rotation switch
            {
                1 => new Point(h - 1 - pos.Y, pos.X),
                2 => new Point(w - 1 - pos.X, h - 1 - pos.Y),
                3 => new Point(pos.Y, w - 1 - pos.X),
                _ => pos
            };
            Point finalDir = Rotation switch
            {
                1 => new Point(-dir.Y, dir.X),
                2 => new Point(-dir.X, -dir.Y),
                3 => new Point(dir.Y, -dir.X),
                _ => dir
            };
            TransformedConnections.Add(new Connection(finalPos, finalDir));
        }
    }

    // Draw method
    public void Draw(SpriteBatch spriteBatch, Vector2 worldOffset, Texture2D pixel)
    {
        Vector2 drawPos = worldOffset + (GridPosition.ToVector2() * Constants.TileSize) + (TransformedSize.ToVector2() * 0.5f * Constants.TileSize);
        
        spriteBatch.Draw(
        BasePiece.Texture,
        drawPos,
        null,
        Color.White,
        MathHelper.ToRadians(Rotation * 90),
        new Vector2(BasePiece.Texture.Width / 2f, BasePiece.Texture.Height / 2f),
        1f,
        IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
        0f
        );

    }
}