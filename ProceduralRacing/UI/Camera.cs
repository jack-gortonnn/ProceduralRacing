using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class Camera
{
    public Vector2 Position;
    public float Zoom;
    public float Rotation;

    private float _previousSpeed;
    private float _forwardOffset;
    private float _previousRotation;
    private float _rotationOffset;

    public Camera(Vector2 startPosition)
    {
        Position = startPosition;
        Zoom = Settings.Camera.ZoomMax;
        Rotation = 0f;
    }

    public void Update(float dt, Car car, Viewport viewport)
    {
        if (dt <= 0f) return;

        UpdatePosition(dt, car);
        UpdateRotation(dt, car);
        UpdateZoom(dt, car);
    }

    private void UpdatePosition(float dt, Car car)
    {
        float currentSpeed = car.Velocity.Length();
        float acceleration = (currentSpeed - _previousSpeed) / dt;
        _previousSpeed = currentSpeed;

        float targetOffset = -acceleration * Settings.Camera.OffsetStrength;
        _forwardOffset = MathHelper.Lerp(_forwardOffset, targetOffset,
            1f - MathF.Exp(-Settings.Camera.OffsetSmoothing * dt));

        Vector2 carForward = new Vector2(
            (float)Math.Cos(car.Rotation),
            (float)Math.Sin(car.Rotation));

        Vector2 desiredPosition = car.Position + carForward * _forwardOffset;
        Position += (desiredPosition - Position) * Settings.Camera.PositionSmoothing * dt;
    }

    private void UpdateRotation(float dt, Car car)
    {
        float angularVelocity = MathHelper.WrapAngle(car.Rotation - _previousRotation) / dt;
        _previousRotation = car.Rotation;

        float targetOffset = -angularVelocity * Settings.Camera.RotationOffsetStrength;
        _rotationOffset = MathHelper.Lerp(_rotationOffset, targetOffset,
            1f - MathF.Exp(-Settings.Camera.RotationOffsetSmoothing * dt));

        float diff = MathHelper.WrapAngle(car.Rotation + _rotationOffset - Rotation);
        Rotation += diff * Settings.Camera.RotationSmoothing * dt;
    }

    private void UpdateZoom(float dt, Car car)
    {
        float speedRatio = MathHelper.Clamp(
            car.Velocity.Length() / Settings.Camera.MaxSpeedReference, 0f, 1f);

        float targetZoom = MathHelper.Lerp(
            Settings.Camera.ZoomMax,
            Settings.Camera.ZoomMin,
            speedRatio);

        Zoom = MathHelper.Lerp(Zoom, targetZoom,
            1f - MathF.Exp(-Settings.Camera.ZoomSmoothing * dt));
    }

    public Matrix GetViewMatrix(Viewport viewport)
    {
        Vector2 screenCenter = new Vector2(
            viewport.Width / 2f,
            viewport.Height * (5f / 6f));

        float rotationWithOffset = -Rotation - MathHelper.PiOver2;

        return
            Matrix.CreateTranslation(-Position.X, -Position.Y, 0f) *
            Matrix.CreateRotationZ(rotationWithOffset) *
            Matrix.CreateScale(Zoom, Zoom, 1f) *
            Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0f);
    }
}