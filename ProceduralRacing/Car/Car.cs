using System;
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

    public Chassis Chassis { get; private set; }
    public Engine Engine { get; private set; }
    public Tyres Tyres { get; private set; }

    private CarPhysics Physics;
    private CarCollision Collision;
    private CarAudio Audio;

    public Vector2 Velocity => Physics.Velocity;
    public float RPM => Physics.RPM;
    public int Gear => Physics.Gear;

    public bool isOnTrack = true;

    public Car(Vector2 startPos, Chassis chassis, Engine engine, Tyres tyres)
    {
        Position = startPos;
        Chassis = chassis;
        Engine = engine;
        Tyres = tyres;

        Physics = new CarPhysics(chassis, engine, tyres);
        Collision = new CarCollision();
        Audio = new CarAudio();
    }

    public void LoadContent(ContentManager content)
    {
        sprite = content.Load<Texture2D>("textures/cars/car");
        origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
        Collision.SetCarDimensions(sprite.Width * Scale * Chassis.collisionScale,
                                   sprite.Height * Scale * Chassis.collisionScale);
        Audio.LoadContent(content);
    }

    public void Update(GameTime gameTime, List<PlacedPiece> track)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        int oldGear = Physics.Gear;

        Physics.Update(dt, Keyboard.GetState(), ref Position, ref Rotation);
        Audio.Update(dt, Physics.RPM, Engine.rpmMax, Physics.IsThrottle, Physics.Gear, oldGear);

        isOnTrack = Collision.IsOnTrack(Position, Rotation, track, Chassis.collisionScale);
        if (!isOnTrack) Physics.Velocity *= (float)Math.Pow(Tyres.oobBrakingPower, dt * 60f);
    }

    public void Draw(SpriteBatch sb) =>
        sb.Draw(sprite, Position, null, Color.White, Rotation, origin, Scale, 0, 0);

    public void ResetCar()
    {
        Position = new Vector2((Settings.Generation.TileSize * 14) + 8, (Settings.Generation.TileSize * 14) + 44);
        Rotation = 0;
        Physics.Velocity = Vector2.Zero;
    }
}