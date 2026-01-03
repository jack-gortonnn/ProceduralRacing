using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


// Placed Pieces are track pieces that have a specific position, rotation, and flip state on the track grid.
// They are the final result of the track generation process and are used for rendering and gameplay.

public class PlacedPiece
{
    // --- Properties ---
    public TrackPiece BasePiece { get; }
    public int Rotation { get; }
    public bool IsFlipped { get; }
    public Point GridPosition { get; }
    public Point TransformedSize { get; }
    public List<Connection> TransformedConnections { get; }

    // --- Generation Helpers ---
    public List<Candidate> RemainingOptions { get; set; } = new List<Candidate>();
    public Connection UsedExitConnection { get; set; }

    // --- Constructor ---
    public PlacedPiece(TrackPiece basePiece, int rotation, bool isFlipped, Point gridPosition)
    {
        BasePiece = basePiece;
        GridPosition = gridPosition;
        Rotation = ((rotation % 4) + 4) % 4;
        IsFlipped = isFlipped;

        TransformedSize = (Rotation % 2 == 0) ? BasePiece.Size : new Point(BasePiece.Size.Y, BasePiece.Size.X);
        TransformedConnections = PieceLibrary.GetTransformedConnections(BasePiece, Rotation, IsFlipped);
    }

    // --- Rendering ---
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
