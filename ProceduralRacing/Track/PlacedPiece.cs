using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

public class PlacedPiece
{
    public TrackPiece BasePiece { get; }
    public int Rotation { get; }
    public bool IsFlipped { get; }
    public Point GridPosition { get; }
    public Point TransformedSize { get; }
    public List<Connection> TransformedConnections { get; }

    // Quick hack to store remaining candidate options for this piece during generation
    // Ideally this wouldn't be here but i want to avoid creating another class just for generation state
    public List<Candidate> RemainingOptions { get; set; } = new List<Candidate>();
    public Connection UsedExitConnection { get; set; }

    public PlacedPiece(TrackPiece basePiece, int rotation, bool isFlipped, Point gridPosition)
    {
        BasePiece = basePiece;
        GridPosition = gridPosition;
        Rotation = ((rotation % 4) + 4) % 4;
        IsFlipped = isFlipped;

        TransformedSize = (Rotation % 2 == 0) ? BasePiece.Size : new Point(BasePiece.Size.Y, BasePiece.Size.X);

        // Again, not ideal to have this logic here since we already call GTC in the generator
        // I could maybe pass in the transformed connections from there instead but meh
        TransformedConnections = BasePiece.GetTransformedConnections(Rotation, IsFlipped);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 drawPos = (GridPosition.ToVector2() * Constants.TileSize) + (TransformedSize.ToVector2() * 0.5f * Constants.TileSize);

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
