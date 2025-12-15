using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

public class PlacedPiece
{
    public TrackPiece BasePiece { get; set; }
    public Point GridPosition { get; set; }
    public int Rotation { get; set; } = 0;
    public bool IsFlipped { get; set; } = false;

    public PlacedPiece(TrackPiece basePiece, Point gridPosition, int rotation, bool isFlipped)
    {
        BasePiece = basePiece;
        GridPosition = gridPosition;
        Rotation = rotation;
        IsFlipped = isFlipped;
    }

    public List<Connection> TransformedConnections
    {
        get
        {
            List<Connection> result = new();
            int width = (Rotation % 2 == 0) ? BasePiece.Size.X : BasePiece.Size.Y;
            int height = (Rotation % 2 == 0) ? BasePiece.Size.Y : BasePiece.Size.X;

            foreach (var c in BasePiece.Connections)
            {
                Point pos = c.Position;
                Point dir = c.Direction;

                // apply rotation
                for (int i = 0; i < Rotation; i++)
                {
                    pos = new Point(pos.Y, width - 1 - pos.X);
                    dir = new Point(dir.Y, -dir.X);
                    int tmp = width;
                    width = height;
                    height = tmp;
                }

                // apply horizontal flip
                if (IsFlipped)
                {
                    pos.X = width - 1 - pos.X;
                    dir.X = -dir.X;
                }

                result.Add(new Connection(pos, dir));
            }

            return result;
        }
    }


    public Point GetAbsolutePosition(Connection connection)
    {
        return new Point(GridPosition.X + connection.Position.X, GridPosition.Y + connection.Position.Y);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 worldOffset)
    {
        var position = worldOffset + GridPosition.ToVector2() * 32;

        SpriteEffects effects = IsFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        float rotationRadians = MathHelper.ToRadians(Rotation * 90);

        spriteBatch.Draw(
            BasePiece.Texture,
            position,
            null,
            Color.White,
            rotationRadians,
            Vector2.Zero, // pivot at top-left
            1f,
            effects,
            0f
        );
    }



}
