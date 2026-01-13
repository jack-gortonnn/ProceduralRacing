using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ProceduralRacing
{
    public class Car
    {
        private Texture2D sprite;
        private Vector2 origin;
        public CarConfig config;

        public Vector2 Position;
        public Vector2 Velocity;
        private float angularVelocity = 0f;
        public float Rotation;
        public float Scale = 0.35f;



        public Car(Vector2 startPosition, CarPreset preset)
        {
            Position = startPosition;
            Rotation = 0f;
            Velocity = Vector2.Zero;
            config = CarConfig.FromPreset(preset);
        }

        public void LoadContent(ContentManager content)
        {
            sprite = content.Load<Texture2D>("textures/cars/car");
            origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }

        public void ResetCar()
        {
            Position = new Vector2((Constants.TileSize * 14) + 8, (Constants.TileSize * 14) + 44);
            Velocity = Vector2.Zero;
            Rotation = 0f;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();

            Vector2 forward = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            Vector2 right = new Vector2(-forward.Y, forward.X);


            float input = (kb.IsKeyDown(Keys.D) ? 1f : 0f) - (kb.IsKeyDown(Keys.A) ? 1f : 0f);
            float speedFactor = 1f / (1f + (Velocity.Length() / config.maxSpeed) * (Velocity.Length() / config.maxSpeed) * 2f);
            angularVelocity = MathHelper.Clamp(
                angularVelocity + (input * config.maxTurnSpeed * speedFactor - angularVelocity)
                * config.turnAcceleration * dt,
                -config.maxTurnSpeed * speedFactor,
                config.maxTurnSpeed * speedFactor
            );

            Rotation += angularVelocity * dt;

            if (kb.IsKeyDown(Keys.W))
            {
                Velocity += forward * config.acceleration * dt;
                if (Velocity.Length() > config.maxSpeed)
                    Velocity = Vector2.Normalize(Velocity) * config.maxSpeed;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                Velocity *= config.brakingPower;
            }

            if (!kb.IsKeyDown(Keys.W) && !kb.IsKeyDown(Keys.S))
            {
                Velocity *= (float)Math.Pow(config.friction, dt * 60f);
            }

            float lateralVelocity = Vector2.Dot(Velocity, right);
            float gripStrength = MathHelper.Lerp(0f, 1.1f, config.gripFactor);
            Velocity -= right * lateralVelocity * gripStrength;

            if (Velocity.Length() < 0.5f)
                Velocity = Vector2.Zero;

            Position += Velocity * dt;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(sprite, Position, null, Color.White, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
