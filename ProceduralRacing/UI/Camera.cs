using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;



    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; } // ADD THIS - rotation in radians
        public float Zoom { get; set; }

        private float zoomSpeed = 0.1f;
        private float minZoom = 0.5f;
        private float maxZoom = 3f;

        public Camera(Vector2 position, float zoom)
        {
            Position = position;
            Zoom = zoom;
            Rotation = 0f; // ADD THISq
        }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            // Zoom controls (optional - can remove if you don't want manual zoom)
            if (keyboard.IsKeyDown(Keys.E) || keyboard.IsKeyDown(Keys.Add))
            {
                Zoom = MathHelper.Clamp(Zoom + zoomSpeed, minZoom, maxZoom);
            }
            if (keyboard.IsKeyDown(Keys.Q) || keyboard.IsKeyDown(Keys.Subtract))
            {
                Zoom = MathHelper.Clamp(Zoom - zoomSpeed, minZoom, maxZoom);
            }
        }

        public Matrix GetViewMatrix(int screenWidth, int screenHeight)
        {
            // Calculate screen center
            Vector2 screenCenter = new Vector2(screenWidth / 2f, screenHeight / 2f);

            // Create transformation matrix:
            // 1. Translate world so car is at origin
            // 2. Rotate around origin
            // 3. Scale by zoom
            // 4. Translate to screen center
            return Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                   Matrix.CreateRotationZ(-Rotation) *  // Negative because we rotate world opposite to car
                   Matrix.CreateScale(Zoom) *
                   Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);
        }
    }
