using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;

public class Camera
{
    public Vector2 Position;
    public float Zoom;

    public Camera(float zoom)
    {
        Zoom = zoom;
    }

    public Matrix GetViewMatrix()
    {
        Vector2 snappedPosition = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));
        float snappedZoom = (float)Math.Round(Zoom * 100f) / 100f;

        return
            Matrix.CreateTranslation(new Vector3(-snappedPosition, 0f)) *
            Matrix.CreateScale(snappedZoom, snappedZoom, 1f);
    }

    // --- Update camera from keyboard input ---
    public void Update(GameTime gameTime, KeyboardState kb)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Zoom
        if (kb.IsKeyDown(Keys.Q)) AddZoom(-0.8f * dt);
        if (kb.IsKeyDown(Keys.E)) AddZoom(0.8f * dt);
    }

    // --- Utility Helpers ---

    public void AddZoom(float delta)
    {
        Zoom = MathHelper.Clamp(Zoom + delta, 0.1f, 10f);
    }

    public void CenterOn(Vector2 target, Viewport viewport)
    {
        Vector2 halfScreen = new Vector2(viewport.Width, viewport.Height) / 2f / Zoom;
        Position = target - halfScreen;
    }
}

