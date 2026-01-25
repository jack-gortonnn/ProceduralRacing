using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ProceduralRacing
{
    public class CarCollision
    {
        private Vector2[] boundingCorners = new Vector2[4];
        private float carWidth;
        private float carHeight;

        public void SetCarDimensions(float width, float height)
        {
            carWidth = width;
            carHeight = height;
        }

        public bool IsOnTrack(Vector2 position, float rotation, List<PlacedPiece> track)
        {
            // Get all pieces the car might be overlapping
            var overlappingPieces = GetOverlappingPieces(position, track);

            if (overlappingPieces.Count == 0)
                return false; // Not over any track piece

            // Check if all corners are on valid track surface
            UpdateBoundingCorners(position, rotation);

            foreach (var corner in boundingCorners)
            {
                bool cornerIsValid = false;

                foreach (var piece in overlappingPieces)
                {
                    if (IsPointOnPiece(corner, piece))
                    {
                        cornerIsValid = true;
                        break;
                    }
                }

                if (!cornerIsValid)
                    return false; // At least one corner is off-track
            }

            return true;
        }

        private void UpdateBoundingCorners(Vector2 position, float rotation)
        {
            float halfWidth = carWidth / 2f;
            float halfHeight = carHeight / 2f;

            // Local space corners (before rotation)
            Vector2[] localCorners = new Vector2[]
            {
                new Vector2(-halfWidth, -halfHeight),
                new Vector2(halfWidth, -halfHeight),
                new Vector2(halfWidth, halfHeight),
                new Vector2(-halfWidth, halfHeight)
            };

            // Rotate and translate to world space
            for (int i = 0; i < 4; i++)
            {
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);

                boundingCorners[i] = new Vector2(
                    localCorners[i].X * cos - localCorners[i].Y * sin,
                    localCorners[i].X * sin + localCorners[i].Y * cos
                ) + position;
            }
        }

        private List<PlacedPiece> GetOverlappingPieces(Vector2 position, List<PlacedPiece> track)
        { // Get pieces overlapping the car's bounding rectangle
            var result = new List<PlacedPiece>();
            Rectangle carBounds = GetBoundingRectangle(position);

            foreach (var piece in track)
            {
                Rectangle pieceWorldBounds = new Rectangle(
                    piece.GridPosition.X * Constants.TileSize,
                    piece.GridPosition.Y * Constants.TileSize,
                    piece.TransformedSize.X * Constants.TileSize,
                    piece.TransformedSize.Y * Constants.TileSize
                );

                if (carBounds.Intersects(pieceWorldBounds))
                    result.Add(piece);
            }

            return result;
        }

        private Rectangle GetBoundingRectangle(Vector2 position)
        {
            float halfWidth = carWidth / 2f;
            float halfHeight = carHeight / 2f;
            float maxExtent = (float)Math.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);

            return new Rectangle(
                (int)(position.X - maxExtent),
                (int)(position.Y - maxExtent),
                (int)(maxExtent * 2),
                (int)(maxExtent * 2)
            );
        }

        private bool IsPointOnPiece(Vector2 worldPoint, PlacedPiece piece)
        { // Check if a world point is on the track surface of the given piece
            Vector2 pieceWorldPos = piece.GridPosition.ToVector2() * Constants.TileSize;
            Vector2 localPoint = worldPoint - pieceWorldPos;
            Vector2 texturePoint = TransformToTextureSpace(localPoint, piece);

            // Sample the actual texture
            int pixelX = (int)texturePoint.X;
            int pixelY = (int)texturePoint.Y;

            // Bounds check
            if (pixelX < 0 || pixelY < 0 ||
                pixelX >= piece.BasePiece.Texture.Width ||
                pixelY >= piece.BasePiece.Texture.Height)
                return false;

            // Sample pixel - if it has color (not transparent) = on track
            Color pixelColor = GetPixelColor(piece.BasePiece.Texture, pixelX, pixelY);
            return pixelColor.A > 128; // Alpha > 128 = on track, transparent = out of bounds
        }

        private Vector2 TransformToTextureSpace(Vector2 localPoint, PlacedPiece piece)
        { // Transform local piece coordinates to texture coordinates
            Vector2 texturePoint = localPoint;

            // Account for rotation (inverse rotation to get back to texture space)
            float angleRad = -MathHelper.ToRadians(piece.Rotation * 90);
            Vector2 center = new Vector2(
                piece.TransformedSize.X * Constants.TileSize / 2f,
                piece.TransformedSize.Y * Constants.TileSize / 2f
            );

            // Rotate around center
            Vector2 offsetFromCenter = texturePoint - center;
            float cos = (float)Math.Cos(angleRad);
            float sin = (float)Math.Sin(angleRad);
            Vector2 rotated = new Vector2(
                offsetFromCenter.X * cos - offsetFromCenter.Y * sin,
                offsetFromCenter.X * sin + offsetFromCenter.Y * cos
            );

            // Account for flip
            if (piece.IsFlipped)
                rotated.X = -rotated.X;

            // Translate back and convert to texture coordinates
            texturePoint = rotated + new Vector2(
                piece.BasePiece.Texture.Width / 2f,
                piece.BasePiece.Texture.Height / 2f
            );

            return texturePoint;
        }

        private Color GetPixelColor(Texture2D texture, int x, int y)
        { // Sample a single pixel color from the texture
            Color[] data = new Color[1];
            texture.GetData(0, new Rectangle(x, y, 1, 1), data, 0, 1);
            return data[0];
        }
    }
}