using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProceduralRacing
{
    public class CarCollision
    {
        private float carWidth;
        private float carHeight;

        public void SetCarDimensions(float width, float height)
        {
            carWidth = width;
            carHeight = height;
        }

        public bool IsOnTrack(Vector2 position, float rotation, List<PlacedPiece> track, float collisionScale)
        { // Returns true if at least two corners of the car's bounding box are on track pieces
            var overlapping = GetOverlappingPieces(position, track);
            if (overlapping.Count == 0) return false;

            int onTrack = GetCorners(position, rotation, collisionScale)
                .Count(corner => overlapping.Any(piece => IsPointOnPiece(corner, piece)));

            return onTrack >= 2;
        }

        private IEnumerable<Vector2> GetCorners(Vector2 position, float rotation, float scale)
        { // Yields the four rotated bounding corners, scaled by the collision multiplier

            float hw = carWidth * scale / 2f;
            float hh = carHeight * scale / 2f;
            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector2[] local = { new(-hw, -hh), new(hw, -hh), new(hw, hh), new(-hw, hh) };

            foreach (var c in local)
                yield return new Vector2(c.X * cos - c.Y * sin, c.X * sin + c.Y * cos) + position;
        }

        private List<PlacedPiece> GetOverlappingPieces(Vector2 position, List<PlacedPiece> track)
        { // Broad phase — filters to pieces whose world bounds intersect the car

            float extent = (float)Math.Sqrt(carWidth * carWidth + carHeight * carHeight) / 2f;
            Rectangle carBounds = new((int)(position.X - extent), (int)(position.Y - extent), (int)(extent * 2), (int)(extent * 2));

            return track.Where(p => new Rectangle(
                p.GridPosition.X * Settings.Generation.TileSize,
                p.GridPosition.Y * Settings.Generation.TileSize,
                p.TransformedSize.X * Settings.Generation.TileSize,
                p.TransformedSize.Y * Settings.Generation.TileSize
            ).Intersects(carBounds)).ToList();
        }

        private bool IsPointOnPiece(Vector2 worldPoint, PlacedPiece piece)
        { // Transforms a world point into texture space and samples the alpha

            Vector2 local = worldPoint - piece.GridPosition.ToVector2() * Settings.Generation.TileSize;
            Vector2 center = new(piece.TransformedSize.X * Settings.Generation.TileSize / 2f, piece.TransformedSize.Y * Settings.Generation.TileSize / 2f);

            float angle = -MathHelper.ToRadians(piece.Rotation * 90);
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            Vector2 o = local - center;
            Vector2 rotated = new(o.X * cos - o.Y * sin, o.X * sin + o.Y * cos);
            if (piece.IsFlipped) rotated.X = -rotated.X;

            Vector2 texPoint = rotated + new Vector2(piece.BasePiece.Texture.Width / 2f, piece.BasePiece.Texture.Height / 2f);

            int px = (int)texPoint.X, py = (int)texPoint.Y;
            if (px < 0 || py < 0 || px >= piece.BasePiece.Texture.Width || py >= piece.BasePiece.Texture.Height) return false;

            Color[] pixel = new Color[1];
            piece.BasePiece.Texture.GetData(0, new Rectangle(px, py, 1, 1), pixel, 0, 1);
            return pixel[0].A > 128;
        }
    }
}