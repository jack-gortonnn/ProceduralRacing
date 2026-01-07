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
        private Vector2 spriteOrigin;
        public float Scale { get; set; } = 0.5f;

        // --- Physics Properties ---
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Rotation { get; private set; }

        // --- Car Stats ---
        private float acceleration = 200f;  
        private float maxSpeed = 150f;  
        private float friction = 0.95f; 
        private float turnSpeed = 4f;
        private float brakingPower = 0.9f;

        // --- Input State ---
        private KeyboardState previousKeyboard;

        public Car(Vector2 startPosition, float startRotation = 0f)
        {
            Position = startPosition;
            Rotation = startRotation;
            Velocity = Vector2.Zero;
        }

        public void LoadContent(ContentManager Content)
        {
            sprite = Content.Load<Texture2D>("textures/cars/chassis/f1");
            spriteOrigin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // --- Input Handling ---
            bool accelerating = keyboard.IsKeyDown(Keys.W);
            bool braking = keyboard.IsKeyDown(Keys.S);
            bool turningLeft = keyboard.IsKeyDown(Keys.A);
            bool turningRight = keyboard.IsKeyDown(Keys.D);

            // --- Steering (only turn when moving) ---
            float currentSpeed = Velocity.Length();
            if (currentSpeed > 5f)
            {
                if (turningLeft) Rotation -= turnSpeed * dt;
                if (turningRight) Rotation += turnSpeed * dt;
            }

            // --- Acceleration ---
            if (accelerating)
            {
                Vector2 forwardDir = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)
                );

                Velocity += forwardDir * acceleration * dt;

                // Cap speed
                if (Velocity.Length() > maxSpeed) Velocity = Vector2.Normalize(Velocity) * maxSpeed;
            }

            // --- Braking ---
            if (braking) Velocity *= brakingPower;

            // --- Apply Friction ---
            if (!accelerating) Velocity *= friction;

            // Stop completely if velocity is very low
            if (Velocity.Length() < 0.5f) Velocity = Vector2.Zero;

            // --- Update Position ---
            Position += Velocity * dt;

            previousKeyboard = keyboard;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, float cameraZoom)
        { // Draws the car at the center of the screen with rotation and scaling
            spriteBatch.Draw(sprite, screenCenter, null, Color.White, 0f, spriteOrigin, Scale * cameraZoom, SpriteEffects.None, 0f);     
        }

        // --- Helper Methods ---

        public void Reset(Vector2 position, float rotation)
        {
            Position = position;
            Rotation = rotation;
            Velocity = Vector2.Zero;
        }
    }
}