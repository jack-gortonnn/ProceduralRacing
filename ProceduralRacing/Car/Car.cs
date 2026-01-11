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
        public float Rotation;
        public float Scale = 0.5f;

        public Car(Vector2 startPosition)
        {
            Position = startPosition;
            Rotation = 0f;
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

            float moveSpeed = 200f;
            float turnSpeed = 3f;

            if (kb.IsKeyDown(Keys.A))
                Rotation -= turnSpeed * dt;

            if (kb.IsKeyDown(Keys.D))
                Rotation += turnSpeed * dt;

            if (kb.IsKeyDown(Keys.W))
            {
                Vector2 forward = new Vector2(
                    (float)Math.Cos(Rotation),
                    (float)Math.Sin(Rotation)
                );

                Position += forward * moveSpeed * dt;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                sprite,
                Position,
                null,
                Color.White,
                Rotation,
                origin,
                Scale,
                SpriteEffects.None,
                0f
            );
        }

    }
}
