using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ProceduralRacing;

public class Car
{
    private Texture2D sprite;
    private Vector2 origin;
    public Vector2 Position;
    public float Rotation;
    public float Scale = 0.35f;

    private CarPhysics physics;
    private CarCollision collision;
    private CarAudio audio;

    public CarConfig Config => physics.config;
    public Vector2 Velocity => physics.Velocity;
    public float RPM => physics.RPM;
    public int Gear => physics.Gear;

    public bool isOnTrack = true;

    public Car(Vector2 startPos, CarPreset preset)
    {
        Position = startPos;
        physics = new CarPhysics(preset);
        collision = new CarCollision();
        audio = new CarAudio();
    }

    public void LoadContent(ContentManager content)
    {
        sprite = content.Load<Texture2D>("textures/cars/car");
        origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
        collision.SetCarDimensions(sprite.Width * Scale, sprite.Height * Scale);
        audio.LoadContent(content);
    }

    public void Update(GameTime gameTime, List<PlacedPiece> track)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        int oldGear = physics.Gear;

        // 1. Physics Update
        physics.Update(dt, Keyboard.GetState(), ref Position, ref Rotation);

        // 2. Audio Update (Logic + Shift Clicks)
        bool shifted = physics.Gear != oldGear;
        audio.Update(physics.RPM, 7500f, shifted, physics.IsThrottle);

        // 3. Track logic
        isOnTrack = collision.IsOnTrack(Position, Rotation, track);
        if (!isOnTrack) physics.Velocity *= physics.config.oobBrakingPower;
    }

    public void Draw(SpriteBatch sb) =>
        sb.Draw(sprite, Position, null, Color.White, Rotation, origin, Scale, 0, 0);

    public void ResetCar()
    {
        Position = new Vector2((Constants.TileSize * 14) + 8, (Constants.TileSize * 14) + 44);
        Rotation = 0;
        physics.Velocity = Vector2.Zero; // Reset via public field
    }
}