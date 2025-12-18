using Microsoft.Xna.Framework;

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
        return
            Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
            Matrix.CreateScale(Zoom, Zoom, 1f);
    }

    // helpers for later
    public void Move(Vector2 delta) => Position += delta;
    public void AddZoom(float delta) => Zoom = MathHelper.Clamp(Zoom + delta, 0.1f, 10f);
}
