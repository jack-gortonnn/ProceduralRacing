using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// Follows the car smoothly. Three effects give a sense of speed:
//   lag/lead - the camera is pushed against the car's acceleration, so it drops behind when speeding up
//              and drifts ahead when braking (showing more of the upcoming corner)
//   zoom     - zooms out as speed increases
//   shake    - shakes while the car is off-track, scaled by speed

public class Camera
{
    public Vector2 Position;
    public float Rotation;
    public float Zoom = Settings.Camera.ZoomMax;

    private Vector2 lastCarVelocity;
    private Vector2 leadOffset;
    private Vector2 shakeOffset, shakeTarget;
    private float shakeTimer;
    private static readonly Random rng = new Random();

    public Camera(Vector2 startPosition) => Position = startPosition;

    public void Update(float dt, Car car)
    {
        if (dt <= 0f) return;

        // Lag / lead: offset opposite to the car's acceleration
        Vector2 carAcceleration = (car.Velocity - lastCarVelocity) / dt;
        lastCarVelocity = car.Velocity;
        Vector2 targetLead = -carAcceleration * Settings.Camera.LeadStrength;
        leadOffset = Vector2.Lerp(leadOffset, targetLead, 1f - MathF.Exp(-3f * dt));

        Vector2 target = car.Position + leadOffset;
        Position = Vector2.Lerp(Position, target, 1f - MathF.Exp(-Settings.Camera.PositionSmoothing * dt));

        // Rotation: ease towards the car's heading, always turning the shortest way round
        float rotationDelta = MathHelper.WrapAngle(car.Rotation - Rotation);
        Rotation = MathHelper.WrapAngle(Rotation + rotationDelta * (1f - MathF.Exp(-Settings.Camera.RotationSmoothing * dt)));

        // Zoom: further out at higher speed
        float speedRatio = MathHelper.Clamp(car.Speed / Settings.Camera.MaxSpeedReference, 0f, 1f);
        float targetZoom = MathHelper.Lerp(Settings.Camera.ZoomMax, Settings.Camera.ZoomMin, speedRatio);
        Zoom = MathHelper.Lerp(Zoom, targetZoom, 1f - MathF.Exp(-Settings.Camera.ZoomSmoothing * dt));

        // Shake: while off-track, pick a new random target ~12 times a second and glide towards it,
        // giving a low, rumbly wobble instead of a jittery snap every frame
        float shake = car.IsOnTrack ? 0f : Settings.Camera.ShakeStrength * MathHelper.Clamp(car.Speed / 40f, 0f, 1f);
        shakeTimer -= dt;
        if (shakeTimer <= 0f)
        {
            shakeTarget = new Vector2((float)rng.NextDouble() * 2 - 1, (float)rng.NextDouble() * 2 - 1) * shake;
            shakeTimer = 1f / 12f;
        }
        shakeOffset = Vector2.Lerp(shakeOffset, shakeTarget * (shake > 0 ? 1f : 0f), 1f - MathF.Exp(-15f * dt));
    }

    public Matrix GetViewMatrix(Viewport viewport)
    { // Puts the car near the bottom of the screen, pointing up, so the player sees more of the road ahead
        Vector2 screenAnchor = new Vector2(viewport.Width / 2f, viewport.Height * (5f / 6f));
        Vector2 cameraPos = Position + shakeOffset;

        return Matrix.CreateTranslation(-cameraPos.X, -cameraPos.Y, 0f)
             * Matrix.CreateRotationZ(-MathHelper.PiOver2 - Rotation)
             * Matrix.CreateScale(Zoom, Zoom, 1f)
             * Matrix.CreateTranslation(screenAnchor.X, screenAnchor.Y, 0f);
    }
}
