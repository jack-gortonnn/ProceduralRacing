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

        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Scale = 0.5f;

        // Car physics
        private float acceleration = 150f;       // units per second^2
        public float maxSpeed = 150f;            // units per second
        private float friction = 0.995f;          // slows car when not accelerating
        private float brakingPower = 0.98f;       // multiply velocity when braking
        private float angularVelocity = 0f;      // current turning speed
        private float turnAcceleration = 8f;     // how fast turning builds up (radians/sec²)
        private float maxTurnSpeed = 3f;         // max turning speed (radians/sec)


        public Car(Vector2 startPosition)
        {
            Position = startPosition;
            Rotation = 0f;
            Velocity = Vector2.Zero;
        }

        public void LoadContent(ContentManager content)
        {
            sprite = content.Load<Texture2D>("textures/cars/car");
            origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState kb = Keyboard.GetState();

            Vector2 forward = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            Vector2 right = new Vector2(-forward.Y, forward.X); // perpendicular to forward


            float input = (kb.IsKeyDown(Keys.D) ? 1f : 0f) - (kb.IsKeyDown(Keys.A) ? 1f : 0f);
            float speedFactor = 1f / (1f + (Velocity.Length() / maxSpeed) * (Velocity.Length() / maxSpeed) * 2f);
            angularVelocity = MathHelper.Clamp(angularVelocity + (input * maxTurnSpeed * speedFactor - angularVelocity) 
                                                      * turnAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds,
                                                               -maxTurnSpeed * speedFactor, maxTurnSpeed * speedFactor);

            Rotation += angularVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kb.IsKeyDown(Keys.W))
            {
                Velocity += forward * acceleration * dt;
                if (Velocity.Length() > maxSpeed)
                    Velocity = Vector2.Normalize(Velocity) * maxSpeed;
            }
            if (kb.IsKeyDown(Keys.S))
            {
                Velocity *= brakingPower;
            }

            if (!kb.IsKeyDown(Keys.W) && !kb.IsKeyDown(Keys.S))
            {
                Velocity *= (float)Math.Pow(friction, dt * 60f);
            }

            float lateralVelocity = Vector2.Dot(Velocity, right);
            Velocity -= right * lateralVelocity * 0.1f;

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
