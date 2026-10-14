using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace ProceduralRacing
{
    public static class CarCollision
    {
        // --- Car vs Track ---

        public static bool IsOnTrack(Car car, List<PlacedPiece> track)
        { // Samples the 4 corners of the car's box against the track pixels. The car only counts as
          // off-track once ALL 4 corners are off (at least one corner must stay on, like real track limits).
            var nearby = GetOverlappingPieces(car, track);
            if (nearby.Count == 0) return false;

            return GetCorners(car).Any(corner => nearby.Any(piece => IsPointOnPiece(corner, piece)));
        }

        // --- Car vs Map Boundary ---

        public static void KeepInsideMap(Car car)
        { // Treats the edge of the grid as a solid wall: clamps the position and kills velocity into the wall
            float max = (Settings.Generation.GridSize + 1) * Settings.Generation.TileSize; // +1 because the last tile is inclusive
            Vector2 pos = car.Physics.Position;
            Vector2 vel = car.Physics.Velocity;

            if (pos.X < 0) { pos.X = 0; vel.X = Math.Max(vel.X, 0); }
            if (pos.Y < 0) { pos.Y = 0; vel.Y = Math.Max(vel.Y, 0); }
            if (pos.X > max) { pos.X = max; vel.X = Math.Min(vel.X, 0); }
            if (pos.Y > max) { pos.Y = max; vel.Y = Math.Min(vel.Y, 0); }

            car.Physics.Position = pos;
            car.Physics.Velocity = vel;
        }

        // --- Car vs Car ---

        public static bool ResolveCarVsCar(Car a, Car b)
        { // Treats each car as a circle. If they overlap, push each one out by half the overlap
          // (fair to both players), then bounce their velocities apart along the line between them.
            Vector2 delta = b.Position - a.Position;
            float distance = delta.Length();
            float minDistance = a.CollisionRadius + b.CollisionRadius;

            if (distance >= minDistance || distance < 0.0001f) return false;

            Vector2 normal = delta / distance;
            float overlap = minDistance - distance;

            // 1. Separate
            a.Physics.Position -= normal * overlap * 0.5f;
            b.Physics.Position += normal * overlap * 0.5f;

            // 2. Bounce (only if they're moving towards each other)
            float closingSpeed = Vector2.Dot(b.Velocity - a.Velocity, normal);
            if (closingSpeed < 0)
            {
                Vector2 impulse = normal * (-(1f + Settings.Race.Restitution) * closingSpeed / 2f);
                a.Physics.Velocity -= impulse;
                b.Physics.Velocity += impulse;
            }
            return true;
        }

        // --- Helpers ---

        private static IEnumerable<Vector2> GetCorners(Car car)
        { // The car's 4 box corners in world space, rotated to match the car
            float hw = car.CollisionWidth / 2f, hh = car.CollisionHeight / 2f;
            float cos = MathF.Cos(car.Rotation), sin = MathF.Sin(car.Rotation);

            Vector2[] local = { new(-hw, -hh), new(hw, -hh), new(hw, hh), new(-hw, hh) };
            foreach (var c in local)
                yield return car.Position + new Vector2(c.X * cos - c.Y * sin, c.X * sin + c.Y * cos);
        }

        private static List<PlacedPiece> GetOverlappingPieces(Car car, List<PlacedPiece> track)
        { // Only test pieces near the car, so we don't sample every piece on the track each frame
            int size = Settings.Generation.TileSize;
            float extent = Math.Max(car.CollisionWidth, car.CollisionHeight);
            Rectangle carBounds = new((int)(car.Position.X - extent), (int)(car.Position.Y - extent), (int)(extent * 2), (int)(extent * 2));

            return track.Where(p => new Rectangle(p.GridPosition.X * size, p.GridPosition.Y * size,
                p.TransformedSize.X * size, p.TransformedSize.Y * size).Intersects(carBounds)).ToList();
        }

        private static bool IsPointOnPiece(Vector2 worldPoint, PlacedPiece piece)
        { // Converts a world point into the piece's texture space (undoing rotation/flip) and checks the pixel isn't transparent
            int size = Settings.Generation.TileSize;
            Vector2 center = new(piece.TransformedSize.X * size / 2f, piece.TransformedSize.Y * size / 2f);
            Vector2 o = worldPoint - piece.GridPosition.ToVector2() * size - center;

            float angle = -MathHelper.ToRadians(piece.Rotation * 90);
            float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
            Vector2 rotated = new(o.X * cos - o.Y * sin, o.X * sin + o.Y * cos);
            if (piece.IsFlipped) rotated.X = -rotated.X;

            var tex = piece.BasePiece.Texture;
            int px = (int)(rotated.X + tex.Width / 2f), py = (int)(rotated.Y + tex.Height / 2f);
            if (px < 0 || py < 0 || px >= tex.Width || py >= tex.Height) return false;

            return piece.BasePiece.PixelData[py * tex.Width + px].A > 128;
        }
    }
}
