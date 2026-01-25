using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProceduralRacing;

public class Camera
{
    public Vector2 Position;   // target world position (car)
    public float Zoom = 1f;
    public float Rotation = 0f;

    public Camera(Vector2 startPosition, float zoom = 1f)
    {
        Position = startPosition;
        Zoom = zoom;
    }

    public void FollowCar(Car car, Viewport viewport, float dt, float smoothing)
    { // Smoothly follow position
        float speedFactor = car.Velocity.Length() / car.Config.maxSpeed; // 0..1
        float smoothingFactor = MathHelper.Lerp(smoothing * 0.5f, smoothing, speedFactor);

        Vector2 screenCenter = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
        Vector2 desiredPosition = car.Position;

        // Smoothly interpolate camera position
        Position += (desiredPosition - Position) * smoothingFactor * dt;
    }

    public void FollowRotation(float targetRotation, float dt, float smoothing)
    { // Smoothly follow rotation
        float diff = MathHelper.WrapAngle(targetRotation - Rotation);
        Rotation += diff * smoothing * dt;
    }

    // Only handles zoom
    public void UpdateZoom(GameTime gameTime, KeyboardState kb)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (kb.IsKeyDown(Keys.Q)) Zoom = MathHelper.Clamp(Zoom - 2f * dt, 0.1f, 10f);
        if (kb.IsKeyDown(Keys.E)) Zoom = MathHelper.Clamp(Zoom + 2f * dt, 0.1f, 10f);
    }

    public Matrix GetViewMatrix(Viewport viewport)
    {
        // 3/4ths down the screen
        Vector2 screenCenter = new Vector2(viewport.Width / 2f, (viewport.Height / 6f)*5f);

        // Apply -90° offset so car's forward is "up" on screen
        float rotationWithOffset = -Rotation - MathHelper.PiOver2;

        // 1: move world so camera target is at origin
        Matrix translateToOrigin = Matrix.CreateTranslation(-Position.X, -Position.Y, 0f);

        // 2: rotate around origin
        Matrix rotation = Matrix.CreateRotationZ(rotationWithOffset);

        // 3: scale around origin
        Matrix scale = Matrix.CreateScale(Zoom, Zoom, 1f);

        // 4: move origin to screen center
        Matrix translateToScreen = Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0f);

        return translateToOrigin * rotation * scale * translateToScreen;
    }
}
