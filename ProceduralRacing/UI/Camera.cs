using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

public class Camera
{
    public Vector2 Position;
    public float Zoom;

    public Camera(Vector2 position, float zoom)
    {
        Position = position;
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

        Vector2 move = Vector2.Zero;

        if (kb.IsKeyDown(Keys.W)) move.Y -= 1f;
        if (kb.IsKeyDown(Keys.S)) move.Y += 1f;
        if (kb.IsKeyDown(Keys.A)) move.X -= 1f;
        if (kb.IsKeyDown(Keys.D)) move.X += 1f;

        Move(move * 600f * dt);

        // Zoom
        if (kb.IsKeyDown(Keys.Q)) AddZoom(-0.8f * dt);
        if (kb.IsKeyDown(Keys.E)) AddZoom(0.8f * dt);
    }

    // --- Utility Helpers ---
    public void Move(Vector2 delta) => Position += delta;

    public void AddZoom(float delta)
    {
        // Clamp zoom to avoid negative or zero
        Zoom = MathHelper.Clamp(Zoom + delta, 0.1f, 10f);
    }

    // later for car
    public void CenterOn(Vector2 target)
    {
        Position = target;
    }
}
