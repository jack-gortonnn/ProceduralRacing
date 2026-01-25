using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ProceduralRacing
{
    public class Car
    {
        private Texture2D sprite;
        private Vector2 origin;

        public Vector2 Position;
        public float Rotation;
        public float Scale = 0.35f;

        private CarPhysics physics;
        private CarCollision collision;

        public bool isOnTrack = true;
        public Vector2 Velocity => physics.Velocity;
        public CarConfig Config => physics.config;

        public Car(Vector2 startPosition, CarPreset preset)
        {
            Position = startPosition;
            Rotation = 0f;

            physics = new CarPhysics(preset);
            collision = new CarCollision();
        }

        public void LoadContent(ContentManager content)
        {
            sprite = content.Load<Texture2D>("textures/cars/car");
            origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);

            collision.SetCarDimensions(sprite.Width * Scale, sprite.Height * Scale);
        }

        public void ResetCar()
        {
            Position = new Vector2((Constants.TileSize * 14) + 8, (Constants.TileSize * 14) + 44);
            Rotation = 0f;
            physics.Reset();
        }

        public void Update(GameTime gameTime, List<PlacedPiece> track)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();

            // Update physics
            physics.Update(dt, kb, ref Position, ref Rotation);

            // Check collision and apply out-of-bounds braking
            isOnTrack = collision.IsOnTrack(Position, Rotation, track);
            if (!isOnTrack)
            {
                physics.ApplyOutOfBoundsBraking();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, Position, null, Color.White, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}